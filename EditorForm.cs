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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Spectrogram;

namespace Typaudio_v2_
{
    public partial class EditorForm : Form
    {
        public SaveFileDialog save = new SaveFileDialog();

        PreviewForm pf;

        public string textRef = "";

        public EditorForm()
        {
            InitializeComponent();
        }

        private void EditorForm_Load(object sender, EventArgs e)
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

            //Text
            label1.Font = wcpFont;
            label2.Font = wcpFont;
            inputTextBox.Font = wcpFont;
            previewTextBox.Font = wcpFont;

            //Buttons
            exportButton.Font = wcpFont;
            previewButton.Font = wcpFont;

            //dgv
            dataGridView.RowsDefaultCellStyle.Font = wcpFont;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = wcpFont;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(253, 231, 36);
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(69, 52, 127);
        }

        /*
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Buttons
         *
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         */
        private void exportButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(Environment.CurrentDirectory + @"\preview.wav"))
            {
                save.Filter = "WAV File (*.wav)|*.wav";
                if (save.ShowDialog() != DialogResult.OK) return;
                File.Copy(Environment.CurrentDirectory + @"\preview.wav", save.FileName);

            }
            else
            {
                AudioFileReader[] audioArray = compileTexttoSound().ToArray();

                var playlist = new ConcatenatingSampleProvider(audioArray);
                //wrap in try catch(IOException) in event that file is opened when repalcing
                save.Filter = "WAV File (*.wav)|*.wav";
                if (save.ShowDialog() != DialogResult.OK) return;

                WaveFileWriter.CreateWaveFile16(save.FileName, playlist);
                MessageBox.Show("Complete");
            }

        }

        private void previewButton_Click(object sender, EventArgs e)
        {
            /*
            
            if (File.Exists(Environment.CurrentDirectory + @"\preview.wav"))
                File.Delete(Environment.CurrentDirectory + @"\preview.wav");

            if (textRef == "")
            {
                MessageBox.Show("You trying to be funny?");
                return;
            }

            AudioFileReader[] audioArray = compileTexttoSound().ToArray();

            var playlist = new ConcatenatingSampleProvider(audioArray);


            (double[] audio, int sampleRate) = ReadWAV(Environment.CurrentDirectory + @"\preview.wav");
            var sg = new SpectrogramGenerator(sampleRate, fftSize: 4096, stepSize: 500, maxFreq: 3000);
            sg.Add(audio);
            //sg.SaveImage("hal.png");

            if (pf != null)
                pf.Close();

            PreviewForm newpf = new PreviewForm();
            newpf.previewPictureBox.Image = sg.GetBitmap();
            newpf.Show();

            pf = newpf;
            //*/
        }

        /*
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
         * Text Change Functions
         * 
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
         */
        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            Regex regex = new Regex(@"^[a-zA-Z ]+$");
            MatchCollection matches = regex.Matches(inputTextBox.Text);
            //if inputs are letters or space
            if (matches.Count > 0 || inputTextBox.Text == "")
            {
                Update_letterPropertiesList(textRef, inputTextBox.Text.ToUpper());
                textRef = inputTextBox.Text.ToUpper();
                previewTextBox.Text = textRef;
                previewTextBox.Text = previewTextBox.Text.Replace(" ", "\"_\"");

            }
            else
            {
                inputTextBox.Text = textRef;
                MessageBox.Show("Invalid Symbol: Must be a letter (Aa - Zz) or Space.");
            }
            //*/
        }

        public void Update_letterPropertiesList(string prevText, string currText)
        {

            //Error Checking
            if (currText == prevText)
                return;

            // Handles letters added
            if (currText.Length > prevText.Length)
            {
                int insertIndex = Math.Abs(inputTextBox.SelectionStart + prevText.Length - currText.Length);
                int subStringLength = currText.Length - prevText.Length;

                for (int i = 0; i < subStringLength; i++)
                {
                    //User Interactable Database
                    dataGridView.Rows.Insert(insertIndex, 1);
                    var newRow = dataGridView.Rows[insertIndex];

                    if (currText[insertIndex] == ' ')
                    {
                        newRow.Cells[0].Value = "_";
                        newRow.ReadOnly = true;
                        return;
                    }
                    else
                    {
                        newRow.Cells[0].Value = currText[insertIndex].ToString();
                        newRow.Cells[1].Value = "Standard";
                        newRow.Cells[2].Value = "O5";
                        newRow.Cells[3].Value = "A";
                        newRow.Cells[4].Value = "Major";

                        var colorBlue = System.Drawing.Color.FromArgb(60, 77, 138);
                        var colorGreen = System.Drawing.Color.FromArgb(124, 210, 79);
                        /*
                        newRow.Cells[0].Style.ForeColor = colorGreen;
                        newRow.Cells[0].Style.BackColor = colorBlue;
                        newRow.Cells[1].Style.ForeColor = colorGreen;
                        newRow.Cells[1].Style.BackColor = colorBlue;
                        newRow.Cells[2].Style.ForeColor = colorGreen;
                        newRow.Cells[2].Style.BackColor = colorBlue;
                        newRow.Cells[3].Style.ForeColor = colorGreen;
                        newRow.Cells[3].Style.BackColor = colorBlue;
                        newRow.Cells[4].Style.ForeColor = colorGreen;
                        newRow.Cells[4].Style.BackColor = colorBlue;
                        //*/
                    }
                    insertIndex++;

                }

            }
            //Handles letters removed
            else
            {
                int removeIndex = inputTextBox.SelectionStart + prevText.Length - currText.Length - 1;
                int subStringLength = prevText.Length - currText.Length;

                for (int i = 0; i < subStringLength; i++)
                {
                    dataGridView.Rows.RemoveAt(removeIndex);
                    removeIndex--;
                }
                //Handles if multiple letters are replaced with a single letter
                if (currText.Length != 0 && currText[removeIndex].ToString() != dataGridView.Rows[removeIndex].Cells[0].Value.ToString())
                {
                    dataGridView.Rows[removeIndex].Cells[0].Value = currText[removeIndex].ToString();
                    dataGridView.Rows[removeIndex].Cells[1].Value = "Standard";
                    dataGridView.Rows[removeIndex].Cells[2].Value = "O5";
                    dataGridView.Rows[removeIndex].Cells[3].Value = "A";
                    dataGridView.Rows[removeIndex].Cells[4].Value = "Major";
                }
            }

        }

        /*
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Access Audio
         * 
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         */

        public static string RowDataToFilePath(string letter, string typeFace, string octave, string note, string chord)
        {
            //Takes _# and puts _S
            if (note.Length == 2)
                return Environment.CurrentDirectory
                            + @"\LetterLibrary\"
                            + octave
                            + "_"
                            + note[0].ToString() + "S"
                            + chord
                            + "_"
                            + typeFace + "_letter-" + letter
                            + ".wav";
            else if (note.Length > 2)

                return Environment.CurrentDirectory
                            + @"\LetterLibrary\O6_A"
                            + chord
                            + "_"
                            + typeFace + "_letter-" + letter
                            + ".wav";
            else
                return Environment.CurrentDirectory
                            + @"\LetterLibrary\"
                            + octave
                            + "_"
                            + note
                            + chord
                            + "_"
                            + typeFace + "_letter-" + letter
                            + ".wav";
        }

        /*
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         * Compiling Audio
         * 
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
         */

        private List<AudioFileReader> compileTexttoSound()
        {
            List<AudioFileReader> buildList = new List<AudioFileReader>();

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                String path;

                if (row.Cells[0].Value.ToString() != "_")
                {
                    path = RowDataToFilePath(row.Cells["Letter"].Value.ToString(),
                                                row.Cells["TypeFace"].Value.ToString(),
                                                row.Cells["Octave"].Value.ToString(),
                                                row.Cells["RootNote"].Value.ToString(),
                                                row.Cells["Chord"].Value.ToString()
                                                );

                }
                else
                {

                    path = Environment.CurrentDirectory + @"\LetterLibrary\letter-space.wav";
                }

                var file = new AudioFileReader(path);
                buildList.Add(file);
            }

            return buildList;
        }

        (double[] audio, int sampleRate) ReadWAV(string filePath, double multiplier = 16_000)
        {
            using var afr = new NAudio.Wave.AudioFileReader(filePath);
            int sampleRate = afr.WaveFormat.SampleRate;
            int sampleCount = (int)(afr.Length / afr.WaveFormat.BitsPerSample / 8);
            int channelCount = afr.WaveFormat.Channels;
            var audio = new List<double>(sampleCount);
            var buffer = new float[sampleRate * channelCount];
            int samplesRead = 0;
            while ((samplesRead = afr.Read(buffer, 0, buffer.Length)) > 0)
                audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
            return (audio.ToArray(), sampleRate);
        }

        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (File.Exists(Environment.CurrentDirectory + @"\preview.wav"))
                File.Delete(Environment.CurrentDirectory + @"\preview.wav");
            
        }
        ////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////
    }
}
