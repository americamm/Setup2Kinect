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

namespace DeteccionBinarios
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //::::::::::::::Variables:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private string archivoColor1 = @"C:\Users\America\Documents\DatosKinect\Bytes02192015165609.file";
        private string archivoDepth1 = @"C:\Users\America\Documents\DatosKinect\Depth02192015162443.file"; 
        private string archivoColor2 = @"C:\Users\America\Documents\DatosKinect\Bytes02192015165609.file";
        private string archivoDepth2 = @"C:\Users\America\Documents\DatosKinect\Depth02192015162443.file"; 

        List<List<short>> DistanciaK1 = new List<List<short>>();
        List<List<short>> DistanciaK2 = new List<List<short>>();
        List<byte[]> FramesK1= new List<byte[]>();
        List<byte[]> FramesK2 = new List<byte[]>(); 
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


        //:::::::::::::Leer y guardar los adtos de los archivos::::::::::::::::::::::::::::::::::::::: 


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


        private List<byte[]> GetFrames(string archivo)
        {
            int lengthValor = 3 * 640 * 480;
            byte[] valor= new byte[lengthValor];
            
            List<byte[]> bytesImagenes = new List<byte[]>(); 

            using(FileStream file = new FileStream(archivo,FileMode.Open,FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(file))
                {
                    while(br.BaseStream.Position != br.BaseStream.Length)
                    {
                        valor = br.ReadBytes(lengthValor);
                        bytesImagenes.Add(valor); 
                    }
                }
            }
            return bytesImagenes;
        } //termina GetFrames() 
    }
}
