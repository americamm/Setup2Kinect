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
        private string archivoColor = @"C:\Users\America\Documents\VideosKinect\02032015123301.avi";
        private string archivoColor2 = @"C:\Users\America\Documents\VideosKinect\02032015123303.avi";
        private string archivoDepth = @"C:\Users\America\Documents\VideosKinect\02032015123302.file";
        private string archivoDepth2 = @"C:\Users\America\Documents\VideosKinect\02032015123304.file";
        List<List<short>> DistanciaK1 = new List<List<short>>();
        List<List<short>> DistanciaK2 = new List<List<short>>(); 
        List<Image<Bgr, Byte>> FramesKinect1 = new List<Image<Bgr, Byte>>();
        List<Image<Bgr, Byte>> FramesKinect2 = new List<Image<Bgr, Byte>>(); 

        //:::::::::::::fin variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::::::::Constructor::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public MainWindow()
        {
            InitializeComponent();

            DistanciaK1 = LeerDepth(archivoDepth);
            DistanciaK2 = LeerDepth(archivoDepth2);

            FramesKinect1 = GetFrames(archivoColor);
            FramesKinect2 = GetFrames(archivoColor2); 
            //GetFrames(); 
        } //Termina constructor 


        //:::::::::::::Leer y guardar los adtos de los archivos::::::::::::::::::::::::::::::::::::::: 


        private List<List<short>> LeerDepth(string archivo)
        {
            short valor;
            List<List<short>> DistanciaK = new List<List<short>>(); 


            using (FileStream file = new FileStream(archivo,FileMode.Open,FileAccess.Read))
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
            bool leevideo = true;
            Image<Bgr,Byte> frame; 
            Capture video = new Capture(archivo);
            List<Image<Bgr, Byte>> allFrames = new List<Image<Bgr, Byte>>();

            while (leevideo)
            {
                frame = video.QueryFrame();
                if (frame != null)
                {
                    allFrames.Add(frame); 
                }
                else
                {
                    leevideo = false; 
                }
            }

            return allFrames;  
        } //termina GetFrames() 


    } //termina class
} //termina 
