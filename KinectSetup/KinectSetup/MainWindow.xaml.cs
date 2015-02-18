using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure; 



namespace KinectSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //::::::::::::::Variables:::::::::::::
        private KinectSensor Kinect;
        private KinectSensorCollection Sensores = KinectSensor.KinectSensors;
        List<KinectSensor> Sensor = new List<KinectSensor>();
        private WriteableBitmap ColorImagenBitmap;
        private WriteableBitmap DepthImagenBitmap;
        private Int32Rect ColorImagenRect;
        private Int32Rect DepthImagenRect;
        private int ColorImagenStride;
        private int DepthImagenStride;
        private byte[] ColorImagenPixeles;
        private byte[] DepthImagenPixeles;
        private short[] DepthValores;
        private System.Drawing.Bitmap BitmapColor;
        bool grabacion = false;
        string directorio = @"C:\Users\America\Documents\VideosKinect\";
        string nameVideo;
        Image<Bgr, Byte> myImagen;
        List<Image<Bgr, Byte>> videoColor1 = new List<Image<Bgr, Byte>>();
        List<Image<Bgr, Byte>> videoColor2 = new List<Image<Bgr, Byte>>();
        List<byte[]> listaBytes1 = new List<byte[]>();
        List<byte[]> listaBytes2 = new List<byte[]>(); 
        List<short[]> DistanciasK1 = new List<short[]>();
        List<short[]> DistanciasK2 = new List<short[]>();
        //:::::::::::::fin variables:::::::::


        //::::::::::::::Constructor:::::::::::::::
        public MainWindow()
        {
            InitializeComponent();
            EncuentraKinects();
            InicilizaKinect();
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }//Termina constructor 


        //::::::::::Enseguida se encuentran los metodos para manipular el flujo de datos del Kinect


        private void EncuentraKinects()
        {
            foreach (KinectSensor Kinect in Sensores)
            {
                if (Kinect.Status == KinectStatus.Connected)
                {
                    Sensor.Add(Kinect);
                }
            }
        } //fin EncuentraKinect()


        private void InicilizaKinect()
        {
            for (int i = 0; i < Sensor.Count; i++)
            {
                Kinect = Sensor[i];
                if (this.Kinect != null)
                {
                    this.Kinect.ColorStream.Enable();
                    this.Kinect.DepthStream.Enable();
                    this.Kinect.DepthStream.Range = DepthRange.Near;
                    this.Kinect.Start();
                }
            }
        } //fin InicializaKinect()


        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            this.ImagenColor.Source = PollColor(0);
            this.Imagen2Color.Source = PollColor(1);
            this.ImagenDepth.Source = PollDepth(0);
            this.Imagen2Depth.Source = PollDepth(1);
        } //fin CompositionTarget_Rendering()


        private WriteableBitmap PollColor(int numKinect)
        {
            Kinect = Sensor[numKinect];
            if (this.Kinect != null)
            {
                ColorImageStream ColorStream = this.Kinect.ColorStream;
                this.ColorImagenBitmap = new WriteableBitmap(ColorStream.FrameWidth, ColorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                this.ColorImagenRect = new Int32Rect(0, 0, ColorStream.FrameWidth, ColorStream.FrameHeight);
                this.ColorImagenStride = ColorStream.FrameWidth * ColorStream.FrameBytesPerPixel;
                this.ColorImagenPixeles = new byte[ColorStream.FramePixelDataLength];

                try
                {
                    using (ColorImageFrame frame = this.Kinect.ColorStream.OpenNextFrame(100))
                    {
                        if (frame != null)
                        {
                            frame.CopyPixelDataTo(this.ColorImagenPixeles);
                            this.ColorImagenBitmap.WritePixels(this.ColorImagenRect, this.ColorImagenPixeles, this.ColorImagenStride, 0);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("No se pueden leer los datos del sensor", "Error");
                }
            }

            if (grabacion == true)
            {
                GuardaImagenes(numKinect, ColorImagenBitmap);
            }

            return ColorImagenBitmap;
        }//fin PollColor() 


        private WriteableBitmap PollDepth(int numKinect)
        {
            Kinect = Sensor[numKinect];
            if (this.Kinect != null)
            {
                DepthImageStream DepthStream = this.Kinect.DepthStream;
                this.DepthImagenBitmap = new WriteableBitmap(DepthStream.FrameWidth, DepthStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                this.DepthImagenRect = new Int32Rect(0, 0, DepthStream.FrameWidth, DepthStream.FrameHeight);
                this.DepthImagenStride = DepthStream.FrameWidth * 4;
                this.DepthValores = new short[DepthStream.FramePixelDataLength];
                this.DepthImagenPixeles = new byte[DepthStream.FramePixelDataLength * 4];

                try
                {
                    using (DepthImageFrame frame = this.Kinect.DepthStream.OpenNextFrame(100))
                    {
                        if (frame != null)
                        {
                            frame.CopyPixelDataTo(this.DepthValores);

                            int index = 0;
                            for (int i = 0; i < frame.PixelDataLength; i++)
                            {
                                int valorDistancia = DepthValores[i] >> 3;

                                if (valorDistancia == this.Kinect.DepthStream.UnknownDepth)
                                {
                                    DepthImagenPixeles[index] = 0;
                                    DepthImagenPixeles[index + 1] = 0;
                                    DepthImagenPixeles[index + 2] = 255;
                                }
                                else if (valorDistancia == this.Kinect.DepthStream.TooFarDepth)
                                {
                                    DepthImagenPixeles[index] = 255;
                                    DepthImagenPixeles[index + 1] = 0;
                                    DepthImagenPixeles[index + 2] = 0;
                                }
                                else
                                {
                                    byte byteDistancia = (byte)(255 - (valorDistancia >> 5));
                                    DepthImagenPixeles[index] = byteDistancia;
                                    DepthImagenPixeles[index + 1] = byteDistancia;
                                    DepthImagenPixeles[index + 2] = byteDistancia;
                                }
                                index = index + 4;
                            }

                            this.DepthImagenBitmap.WritePixels(this.DepthImagenRect, this.DepthImagenPixeles, this.DepthImagenStride, 0);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("No se pueden leer los datos del sensor", "Error");
                }

            }

            if (grabacion == true)
            {
                GuardaDistancias(numKinect, DepthValores);
            }

            return DepthImagenBitmap;
        }//fin PollDepth() 


        //::::::::::::::::::::::::::Guardar imagenes en listas::::::::::::::::::::::::::::::::::::::::::::::


        private System.Drawing.Bitmap convertWriteablebitmap(WriteableBitmap wbitmap)
        {
            System.Drawing.Bitmap returnbitmap;
            using (MemoryStream Stream = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)wbitmap));
                encoder.Save(Stream);
                returnbitmap = new System.Drawing.Bitmap(Stream);
            }
            return returnbitmap;
        }  //finaliza convertWriteableBitmap()


        private void GuardaImagenes(int num, WriteableBitmap ColorImagenBitmap)
        {
            BitmapColor = convertWriteablebitmap(ColorImagenBitmap);
            myImagen = new Image<Bgr, Byte>(BitmapColor);
            if (Sensor.Count == 1)
            {
                videoColor1.Add(myImagen);
            }
            else
            {
                if (num == 0)
                {
                    videoColor1.Add(myImagen);
                    listaBytes1.Add(myImagen.Bytes);
                }
                else
                {
                    videoColor2.Add(myImagen);
                    listaBytes2.Add(myImagen.Bytes);
                }
            }
        } //fin GuardaImagenes() 


        //:::::::::::::::::::::Guarda los datos de las distancias en listas::::::::::::::::::::::::::::::::::

        private void GuardaDistancias(int num, short[] depth)
        {
            if (num == 0)
            {
                DistanciasK1.Add(depth);
            }
            else
            {
                DistanciasK2.Add(depth);
            }
        } //GuardaDistancias();

        //::::::::::::::::::::::::::::Guarda el video y el archivo de distancias::::::::::::::::::::::::::::::::


        private void Grabar_Click(object sender, RoutedEventArgs e)
        {
            grabacion = true;
        }

        private void StopGrabacion_Click(object sender, RoutedEventArgs e)
        {
            for (int n = 0; n < Sensor.Count; n++)
            {
                Record(n);
                RecordDistancia(n);
                RecordBytesImagen(n);
            }
            grabacion = false;
        }


        private void Record(int num)
        {
            nameVideo = String.Format("{0}{1}{2}", directorio, DateTime.Now.ToString("MMddyyyyHmmss"), ".avi");

            if (num == 0)
            {
                using (VideoWriter vi = new VideoWriter(nameVideo, 0, 30, 640, 480, true))
                {
                    for (int i = 0; i < videoColor1.Count(); i++)
                        vi.WriteFrame<Bgr, Byte>(videoColor1[i]);
                    vi.Dispose();
                }

                nameVideo = string.Empty;
                videoColor1.Clear();
            }
            else
            {
                using (VideoWriter vi = new VideoWriter(nameVideo, 0, 30, 640, 480, true))
                {
                    for (int i = 0; i < videoColor2.Count(); i++)
                        vi.WriteFrame<Bgr, Byte>(videoColor2[i]);
                    vi.Dispose();
                }

                nameVideo = string.Empty;
                videoColor2.Clear();
            }
        } //fin Record() 


        private void RecordBytesImagen(int num)
        {
            nameVideo = String.Format("{0}{1}{2}", directorio, DateTime.Now.ToString("MMddyyyyHmmss"), ".file");

            if (num == 0)
            {
                using (FileStream file = new FileStream(nameVideo, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(file))
                    {
                        foreach (byte[] arregloImagen in listaBytes1)
                        {
                            bw.Write(arregloImagen);
                            bw.Write("\n");
                        }
                    }
                }

                nameVideo = null;
                listaBytes1.Clear(); 

            }
            else
            {
                using (FileStream file = new FileStream(nameVideo, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(file))
                    {
                        foreach (byte[] arregloImagen2 in listaBytes2)
                        {
                            bw.Write(arregloImagen2);
                            bw.Write("\n");
                        }
                    }
                }

                nameVideo = null; 
                listaBytes2.Clear();

            }
        }


        private void RecordDistancia(int num)
        {
            nameVideo = String.Format("{0}{1}{2}", directorio, DateTime.Now.ToString("MMddyyyyHmmss"), ".file");

            if (num == 0)
            {
                using (FileStream file = new FileStream(nameVideo, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(file))
                    {
                        foreach (short[] arregloDistancia in DistanciasK1)
                        {
                            foreach (short valor in arregloDistancia)
                            {
                                bw.Write(valor);
                            }
                            bw.Write("\n"); 
                        }
                    }
                }
                nameVideo = string.Empty;
                DistanciasK1.Clear();
            }
            else
            {
                using (FileStream file = new FileStream(nameVideo, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(file))
                    {
                        foreach (short[] arregloDistancia in DistanciasK2)
                        {
                            foreach (short valor in arregloDistancia)
                            {
                                bw.Write(valor);
                            }
                            bw.Write("\n"); 
                        }
                    }
                }
                nameVideo = string.Empty;
                DistanciasK2.Clear();
            }
        }

       //fin RecordDistancias() 




    } //fin class
} //fin de namespace 
