using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.IO;
using UnityEngine.Rendering;
using System.Linq;

[System.Serializable]
public class Ambiculus : MonoBehaviour
{
    [System.Serializable]
    public class LEDPixel
    {
        public int r;
        public int c;
        public LEDPixel(int r, int c)
        {
            this.r = r;
            this.c = c;
        }
    }
    public static byte SET_PIXELS_COLOR = 0;
    public static byte SET_STRIP_COLOR = 1;
    public static byte UPDATE_STRIP = 2;
    public static SerialPort sp;
    public static byte[] data = new byte[5];
    public string comPort = "COM3";
    public int width = 2;
    public int height = 2;
    public AmbiculusImageEffect ambiculusImageEffect;

    //public RenderTexture left,right;

    public Texture2D myTexture2D;
    public GameObject go;
    public float multiplier = 1.0f;
    public List<LEDPixel> pixels = new List<LEDPixel>();


    private Object thisLock = new Object();
    /// <summary>
    /// tells the system the data was sent and a new update can be done
    /// </summary>
	bool needToUpdateLEDs = true;
    byte[] serialPortBuffer;
    Thread sendThread;
    bool run = true;

    public bool sendSignals = true;


    #region computeshader
    Queue<AsyncGPUReadbackRequest> _requests = new Queue<AsyncGPUReadbackRequest>();

    Color32[] outputArray;

    int[] indices;

    #endregion

    // Use this for initialization
    void Start()
    {
        serialPortBuffer = new byte[pixels.Count * 5];

        if (sendSignals)
        {
            sp = new SerialPort(comPort, 57600, Parity.None, 8, StopBits.One);

            OpenConnection();

            sendThread = new Thread(SendLoop);
            sendThread.Start();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (ambiculusImageEffect.left == null)
        {
            return;
        }
        //Initialize
        if (myTexture2D == null)
        {
            Debug.Log("setting up Shaders");
            myTexture2D = new Texture2D(ambiculusImageEffect.left.width, ambiculusImageEffect.left.height)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };

            indices = new int[width * height];
            // set all to -1
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = -1;
            }
            // set the pixel ones accordingly			
            for (int i = 0; i < pixels.Count; i++)
            {
                indices[pixels[i].r * ambiculusImageEffect.left.width + pixels[i].c] = i;
            }
        }
        else
        {

            if (needToUpdateLEDs || !sendSignals)
            {
                lock (thisLock)
                {
                    needToUpdateLEDs = false;
                }
                if (_requests.Count < 8)
                {
                    _requests.Enqueue(AsyncGPUReadback.Request(ambiculusImageEffect.left));
                    cou = StartCoroutine(GetPix());
                }
                else
                    Debug.Log("Too many requests.");
            }
            lock (thisLock)
            {
                if (outputArray != null)
                {
                    for (int li = 0; li < outputArray.Length; li++)
                    {
                        int i = indices[li];
                        if(i>=0 && i< serialPortBuffer.Length)
                        {
                            serialPortBuffer[i * 5 + 0] = SET_PIXELS_COLOR;
                            Color32 c = outputArray[li];
                            serialPortBuffer[i * 5 + 1] = (byte)i;
                            serialPortBuffer[i * 5 + 2] = System.Convert.ToByte((c.r) * multiplier);
                            serialPortBuffer[i * 5 + 3] = System.Convert.ToByte((c.g) * multiplier);
                            serialPortBuffer[i * 5 + 4] = System.Convert.ToByte((c.b) * multiplier);
                            myTexture2D.SetPixel(pixels[i].c, pixels[i].r, new Color(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f));
                        //    Debug.Log("reverseIndex: " + i + " arraylength: " + outputArray.Length + " color: "+ c +" c "+ pixels[i].c + " r "+ pixels[i].r  );
                        }                        
                    }
                    myTexture2D.Apply();
                }                
            }
        }
    }
    /// <summary>
    /// Coroutine for the async GPU readback
    /// </summary>
    /// <returns></returns>
	IEnumerator GetPix()
    {
        coroutineIsRunning = true;
        while (_requests.Count > 0)
        {
            var req = _requests.Peek();

            if (req.hasError)
            {
                Debug.Log("GPU readback error detected.");
                _requests.Dequeue();
            }
            else if (req.done)
            {
                //Debug.Log("GPU readback success.");
                var buffer = req.GetData<Color32>();
                outputArray = buffer.ToArray();
                _requests.Dequeue();
            }
            else
            {
                yield return null;
            }
        }
        coroutineIsRunning = false;
    }

    private void SetStripColor(byte red, byte green, byte blue)
    {
        data[0] = SET_STRIP_COLOR;
        data[1] = 255; // index is discarded 
        data[2] = red;
        data[3] = green;
        data[4] = blue;

        sp.Write(data, 0, data.Length);
    }

    private void OpenConnection()
    {
        if (sp != null)
        {
            if (sp.IsOpen)
            {
                sp.Close();
                Debug.Log("Closing port, because it was already open!");
            }
            else
            {
                //reset arduino : https://arduino.stackexchange.com/questions/4696/how-to-reset-arduino-from-software
                sp.DtrEnable = true;
                sp.Open();
                Thread.Sleep(1000);
                sp.DtrEnable = false;
                //and of course open the port...
                sp.ReadTimeout = 50;  // sets the timeout value before reporting error

                Debug.Log("Serial Port Opened!");
            }
        }
        else
        {
            if (sp.IsOpen)
            {
                Debug.Log("Port is already open");
            }
            else
            {
                Debug.Log("Port == null");
            }
        }
    }

    void SendLoop()
    {
        Debug.Log("Starting SendLoop");
        SetStripColor(0, 0, 0);
        SetStripColor(0, 0, 0);
        SetStripColor(0, 0, 0);
        Thread.Sleep(10);
        SetStripColor(0, 0, 0);
        Thread.Sleep(10);

        while (run)
        {
            //copy array
            byte[] arraycopy = new byte[serialPortBuffer.Length];
            lock (thisLock)
            {
                serialPortBuffer.CopyTo(arraycopy, 0);
                needToUpdateLEDs = true;
            }
            //Debug.Log(string.Join(",",arraycopy.Select(x => x.ToString()).ToArray())); //output of the buffer for debugging.
            sp.Write(arraycopy, 0, arraycopy.Length);
            //SetStripColor(255, 255, 255);
            Thread.Sleep(10);
            //repaint
            byte[] dat = new byte[5];
            dat[0] = UPDATE_STRIP;
            dat[1] = dat[2] = dat[3] = dat[4] = 255; // index and RGB values are discarded 
            sp.Write(dat, 0, dat.Length);
            Thread.Sleep(10);
        }
    }


    void OnDestroy()
    {
        Debug.Log("Shutting down");
        run = false;
        Thread.Sleep(10);
        if (sendThread != null && sendThread.IsAlive)
        {
            try
            {
                sendThread.Abort();
            }
            catch
            {

            }
        }
        byte[] b = new byte[5];
        b[0] = SET_STRIP_COLOR;
        b[1] = b[2] = b[3] = b[4] = 0; // index and RGB values are discarded 
        if (sp != null)
        {
            sp.Write(b, 0, b.Length);
            data[0] = UPDATE_STRIP;
            data[1] = data[2] = data[3] = data[4] = 255; // index and RGB values are discarded 
            sp.Write(data, 0, data.Length);
            Thread.Sleep(100);
            sp.Write(data, 0, data.Length);
            Thread.Sleep(100);
            sp.Write(data, 0, data.Length);
            Thread.Sleep(100);
            sp.Close();
            Thread.Sleep(100);
        }
    }
}

