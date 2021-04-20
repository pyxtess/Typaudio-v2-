using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace Typaudio_v2_
{
    public partial class PreviewForm : Form
    {
        private WaveFileReader previewWave;

        private DirectSoundOut output;
        
        public PreviewForm()
        {
            InitializeComponent();
        }

        private void PreviewForm_Load(object sender, EventArgs e)
        {
            //Create your private font collection object.
            PrivateFontCollection pfc = new PrivateFontCollection();

            //Select your font from the resources.
            //My font here is "Digireu.ttf"
            int fontLength = Properties.Resources.windows_command_prompt.Length;

            // create a buffer to read in to
            byte[] fontdata = Properties.Resources.windows_command_prompt;

            // create an unsafe memory block for the font data
            System.IntPtr data = Marshal.AllocCoTaskMem(fontLength);

            // copy the bytes to the unsafe memory block
            Marshal.Copy(fontdata, 0, data, fontLength);

            // pass the font to the font collection
            pfc.AddMemoryFont(data, fontLength);

            //Applying Font
            //Making Common Var
            var wcpFont = new Font(pfc.Families[0], 10);

            //Button
            button1.Font = wcpFont;


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Play")
            {
                if (!File.Exists(Environment.CurrentDirectory + @"\preview.wav"))
                {
                    MessageBox.Show("Error: Preview WAV file was not found");
                    return;
                }

                previewWave = new WaveFileReader(Environment.CurrentDirectory + @"\preview.wav");
                output = new DirectSoundOut();

                output.Init(new WaveChannel32(previewWave));
                output.Play();
                button1.Text = "Stop";
            }
            else
            {
                output.Stop();
                button1.Text = "Play";
                //DisposeAll();
            }
        }

        private void DisposeAll()
        {
            if (output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing)
                    output.Stop();
                output.Dispose();
                output = null;
            }
            if (previewWave != null)
            {

                previewWave.Dispose();
                previewWave = null;

            }

        }

        private void PreviewForm_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            //DisposeAll();
        }
    }
}
