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
using Microsoft.Kinect; //Creo que aqui no lo usare :/
using System.IO; 
using Emgu.CV; 
using Emgu.CV.Util;
using Emgu.CV.Structure; 



namespace Deteccion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        //::::::::::::::Variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //private string directorio = @;
        private string archivoColor = @"C:\Users\America\Documents\VideosKinect\02032015123309.avi";
        private string archivoDepth = @"C:\Users\America\Documents\VideosKinect\02032015123309.file";
        List<List<short>> DistanciaK1 = new List<List<short>>();
        List<Image<Bgr, Byte>> FramesKinect1 = new List<Image<Bgr, byte>>(); 
        //short[] aDistancia; 
       
        //:::::::::::::fin variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::::::::Constructor::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public MainWindow()
        {
            InitializeComponent();
        } //Termina constructor 


        //:::::::::::::Leer y guardar los adtos de los archivos::::::::::::::::::::::::::::::::::::::: 


        private void LeerDepth()
        {
            short valor; 

            using (FileStream file = new FileStream(archivoDepth,FileMode.Open,FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(file))
                { 
                
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        valor=br.ReadInt16();
                        List<short> renglonLista =new List<short>(); 
                        while (true)
                        {
                            if (valor != 2561)
                            {
                                renglonLista.Add(valor); 
                            }
                            else
                            {
                                DistanciaK1.Add(renglonLista); 
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
                     
                } //end BinaryReader
            } //end FileStream
        } //Termina LeerDepth 


        private void GetFrames()
        {
            bool leevideo = true;
            Image<Bgr,Byte> frame; 
            Capture video = new Capture(archivoColor);

            while (leevideo)
            {
                frame = video.QueryFrame();
                if (frame != null)
                {
                    FramesKinect1.Add(frame); 
                }
                else
                {
                    leevideo = false; 
                }
            }           
        }


    } //termina class
} //termina 
