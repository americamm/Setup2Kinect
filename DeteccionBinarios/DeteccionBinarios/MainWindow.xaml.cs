﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO; 
using Emgu.CV; 
using Emgu.Util; 
using Emgu.CV.Structure; 
using System.Runtime.InteropServices; 

namespace DeteccionBinarios
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //::::::::::::::Variables:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private string archivoColor1 = @"C:\DatosKinect\K1Bytes02262015110709.file";
        private string archivoDepth1 = @"C:\DatosKinect\K1Depth02262015110707.file";
        private string archivoColor2 = @"C:\DatosKinect\K2Bytes02262015110721.file";
        private string archivoDepth2 = @"C:\DatosKinect\K2Depth02262015110720.file"; 

        List<List<short>> DistanciaK1 = new List<List<short>>();
        List<List<short>> DistanciaK2 = new List<List<short>>();
        List<Image<Bgr,Byte>> FramesK1= new List<Image<Bgr,Byte>>(); 
        List<Image<Bgr, Byte>> FramesK2 = new List<Image<Bgr, Byte>>();

        private int filas = 480;        //Para cualquier imagen 
        private int columnas = 640; 
        //:::::::::::::fin variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::::::::Constructor::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public MainWindow()
        {
            InitializeComponent();
            DistanciaK1 = LeerDepth(archivoDepth1);
            DistanciaK2 = LeerDepth(archivoDepth2); 
            FramesK1 = GetFrames(archivoColor1);
            FramesK2 = GetFrames(archivoColor2);
            SkinColorSegmentation(FramesK1);  
        } //Termina constructor 


        //:::::::::::::Leer y guardar los datos de los archivos::::::::::::::::::::::::::::::::::::::: 


        private List<List<short>> LeerDepth(string archivo)
        {
            short valor;
            List<List<short>> DistanciaK = new List<List<short>>();


            using (FileStream file = new FileStream(archivo, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(file))
                {

                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        valor = br.ReadInt16();
                        List<short> renglonLista = new List<short>();
                        while (true)
                        {
                            if (valor != 2561)
                            {
                                renglonLista.Add(valor);
                            }
                            else
                            {
                                DistanciaK.Add(renglonLista);
                            }
                            try
                            {
                                valor = br.ReadInt16();
                            }
                            catch (Exception e)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return DistanciaK;
        } //Termina LeerDepth 


        private List<Image<Bgr,Byte>> GetFrames(string archivo)
        {
            int lengthValor = 3 * 640 * 480;
            byte[] valor= new byte[lengthValor];
            List<Image<Bgr, Byte>> imagenBack = new List<Image<Bgr, Byte>>();
            
            //List<byte[]> bytesImagenes = new List<byte[]>(); 

            using(FileStream file = new FileStream(archivo,FileMode.Open,FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(file))
                {
                    while(br.BaseStream.Position != br.BaseStream.Length)
                    {
                        valor = br.ReadBytes(lengthValor);

                        IntPtr unmanagedPointer = Marshal.AllocHGlobal(valor.Length);
                        Marshal.Copy(valor, 0, unmanagedPointer, valor.Length);
                        imagenBack.Add(new Image<Bgr, Byte>(640, 480, 1920, unmanagedPointer).Copy()) ;

                        unmanagedPointer = IntPtr.Zero; 
                        Marshal.FreeHGlobal(unmanagedPointer);
                    }
                }
            }  
            
            return imagenBack;
        } //termina GetFrames() 


        //:::::::Aqui empieza la parte del programa que hace la deteccion:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //basados en el articulo Hand gesture tracking Using Adaptative Kalman Filter:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        private List<byte[,,]> getBytesYCbCr(List<Image<Bgr,Byte>> framesBgr) 
        {
            List<byte[, ,]> bytesFramesYCbCr = new List<byte[, ,]>(framesBgr.Count);
   
            foreach(Image<Bgr,Byte> frame in framesBgr)
            {
                bytesFramesYCbCr.Add(frame.Convert<Ycc, Byte>().Data);   
            }

            return bytesFramesYCbCr;
        }// termina Convert YCbCr


        private void SkinColorSegmentation(List<Image<Bgr, Byte>> framesBgr)
        {
            List<Image<Gray, Byte>> framesBinarios = new List<Image<Gray, Byte>>(framesBgr.Count);
            List<byte[, ,]> bytesFramesYCC = new List<byte[, ,]>(framesBgr.Count);
            Image<Gray, Byte> frameBi = new Image<Gray, Byte>(filas,columnas); ;
            double mediaCr = 149.7692;
            double mediaCb = 114.3846;
            double deCr = 13.80914;
            double deCb = 7.136041;  
                
            bytesFramesYCC = getBytesYCbCr(framesBgr);
            double izqCr = mediaCr - deCr;
            double derCr = mediaCr + deCr;
            double izqCb = mediaCb - deCb; 
            double derCb = mediaCb + deCb; 


            foreach (byte[,,] arregloBytes in bytesFramesYCC ) 
            {

                for (int i = 0; i < filas; i++ )
                {
                    for (int j = 0; j < columnas; j++)
                    {
                        if ((izqCr < arregloBytes[i, j, 1]) && (arregloBytes[i, j, 1]< derCr) && (izqCb < arregloBytes[i,j,2]) && (arregloBytes[i,j,2]<derCb) )
                        {
                            frameBi.Data[i, j,0] = 255;
                        }
                        else
                        {
                            frameBi.Data[i, j, 0] = 0; 
                        }
                    }
                }

                framesBinarios.Add(frameBi); 
            }

        }// termina getBytesYCbCr 


    }// termina clase
}// termina namespace
