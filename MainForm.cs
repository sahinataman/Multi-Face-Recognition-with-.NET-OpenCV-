

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Diagnostics;

namespace MultiFaceRec
{
    public partial class FrmPrincipal : Form
    {
        //Tüm değişkenleri tanımlama : vectors and haarcascades
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels= new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, names = null;


        public FrmPrincipal()
        {
            InitializeComponent();
            //Yüz algılamaları için haarcascades yükle
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            //eye = new HaarCascade("haarcascade_eye.xml");
            try
            {
                //Her resim için önceden kayıtlı yüzlerin ve etiketlerin yüklenmesi
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/KayitliKisiler.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels+1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                }
            
            }
            catch(Exception e)
            {
                //Yüz tanıma için bilgilendirme mesajı ekrana yazdırma
                MessageBox.Show("Veritabanında kayıtlı hiçbir FACE bulunmamaktadır.Lütfen bir FACE ekleyiniz", "DİKKAT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {

        }

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Cameranın çalıştırılması
            grabber = new Capture();
            grabber.QueryFrame();
            //Camera çerceve olayının çalıştırılması
            Application.Idle += new EventHandler(FrameGrabber);
            button1.Enabled = false;
        }


        private void button2_Click(object sender, System.EventArgs e)
        {
            try
            {
                //Eklenecek alandaki FACE sayacı
                ContTrain = ContTrain + 1;

                //Camera da  Gri cerceve aliyoruz
                gray = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                //Yüz tanımaa
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                face,
                1.2,
                10,
                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                new Size(20, 20));

                //Algılanan her cerceve için yapılacak işlemler
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();
                    break;
                }

                //Tespit edilen cercevenin onceki kayıtlı cercevelerle karşılaştırılması
                TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                trainingImages.Add(TrainedFace);
                labels.Add(textBox1.Text);

                //Yüzü gri tonlama ile gösterme
                imageBox1.Image = TrainedFace;

                File.WriteAllText(Application.StartupPath + "/TrainedFaces/KayitliKisiler.txt", trainingImages.ToArray().Length.ToString() + "%");

                //Cercevelerin karşılaştırılma kısmı
                for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                {
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                    File.AppendAllText(Application.StartupPath + "/TrainedFaces/KayitliKisiler.txt", labels.ToArray()[i - 1] + "%");
                }

                MessageBox.Show(textBox1.Text + "´nın yüzü algılandı ve kaydedildi :)", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Önce yüz algılamayı etkinleştir", " Hata", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        void FrameGrabber(object sender, EventArgs e)
        {
            label3.Text = "0";
            //label4.Text = "";
            NamePersons.Add("");


            //O anki camera boyutları
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            //Gri ye çevirme
            gray = currentFrame.Convert<Gray, Byte>();

            //Yüz Tanıma
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                 face,
                 1.2,
                 10,
                 Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                 new Size(50, 50));

            //Algılanan her öğe için yapılacak işlemler
            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //Tespit edilen yüzün kırmızı kare içerisine alınması 
                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);

                if (trainingImages.ToArray().Length != 0)
                {
                    //Yüz Tanıma için TermCriteria yaratma
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);
                    //Yüz tanıyıcısı
                    FaceRecognizer recognizer = new FaceRecognizer(
                           trainingImages.ToArray(),
                           labels.ToArray(),
                           3000,
                           ref termCrit);
                    name = recognizer.Recognize(result);
                    //Cercevenin üzerindeki yazı (Kişi Tanıyıcı)
                    currentFrame.Draw(name, ref font, new Point(f.rect.X -5,f.rect.Y - 5), new Bgr(Color.LightGreen));

                }

                NamePersons[t - 1] = name;
                NamePersons.Add("");


                //Camera da algılanan yüz sayısı
                label3.Text = facesDetected[0].Length.ToString();

            }
            t = 0;

            //Tanımlanan kişilerin isimlerinin birleştirilmesi
            for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
            {
                names = names + NamePersons[nnn] + ", ";
            }
            //Tanınan Yüzü gösterme
            imageBoxFrameGrabber.Image = currentFrame;
            label4.Text = names;
            names = "";
            //İsim listesini temizleme
            NamePersons.Clear();

        }

    }
}