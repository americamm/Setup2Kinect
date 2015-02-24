using System;
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
        private string archivoColor1 = @"C:\DatosKinect\K1Bytes02232015191607.file";
        private string archivoDepth1 = @"C:\DatosKinect\K1Depth02232015191605.file";
        private string archivoColor2 = @"C:\DatosKinect\K2Bytes02232015191611.file";
        private string archivoDepth2 = @"C:\DatosKinect\K2Depth02232015191610.file"; 

        List<List<short>> DistanciaK1 = new List<List<short>>();
        List<List<short>> DistanciaK2 = new List<List<short>>();
        List<Image<Bgr,Byte>> FramesK1= new List<Image<Bgr,Byte>>(); 
        List<Image<Bgr, Byte>> FramesK2 = new List<Image<Bgr, Byte>>(); 
        //:::::::::::::fin variables::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        //:::::::::::::Constructor::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public MainWindow()
        {
            InitializeComponent();
            DistanciaK1 = LeerDepth(archivoDepth1);
            DistanciaK2 = LeerDepth(archivoDepth2); 
            FramesK1 = GetFrames(archivoColor1);
            FramesK2 = GetFrames(archivoColor2);
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



    }// termina clase
}// termina namespace
