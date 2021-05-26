using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using PexelsDotNetSDK.Api;
using Aspose.Slides;

namespace SEH_TestApplication
{
    public partial class PowerPointCreator : Form
    {
        public List<string> ImageURLs { get; set; }

        public PowerPointCreator()
        {
            InitializeComponent();
        }

        private void BoldButton_Click(object sender, EventArgs e)
        {
            int selStart = DescriptionTextBox.SelectionStart;
            int selLength = DescriptionTextBox.SelectionLength;

            //Toggle font style
            //  This method does not account for mixed styles in a selected string.
            //  That is a solvable problem, but would require much more time and code.
            //If selected string FontStyle does not contain "Bold", adds Bold.
            if (!DescriptionTextBox.SelectionFont.Style.ToString().Contains("Bold"))
                DescriptionTextBox.SelectionFont = new Font(DescriptionTextBox.Font, DescriptionTextBox.SelectionFont.Style | FontStyle.Bold);
            //If selected string FontStyle already contains "Bold", sets to FontStyle minus Bold.
            else
                DescriptionTextBox.SelectionFont = new Font(DescriptionTextBox.Font, DescriptionTextBox.SelectionFont.Style & ~FontStyle.Bold);

            //Reselect text
            DescriptionTextBox.Select(selStart, selLength);
        }

        private void ItalicsButton_Click(object sender, EventArgs e)
        {
            int selStart = DescriptionTextBox.SelectionStart;
            int selLength = DescriptionTextBox.SelectionLength;

            if (!DescriptionTextBox.SelectionFont.Style.ToString().Contains("Italic"))
                DescriptionTextBox.SelectionFont = new Font(DescriptionTextBox.Font, DescriptionTextBox.SelectionFont.Style | FontStyle.Italic);
            else
                DescriptionTextBox.SelectionFont = new Font(DescriptionTextBox.Font, DescriptionTextBox.SelectionFont.Style & ~FontStyle.Italic);

            DescriptionTextBox.Select(selStart, selLength);
        }

        private void StrikethroughButton_Click(object sender, EventArgs e)
        {
            int selStart = DescriptionTextBox.SelectionStart;
            int selLength = DescriptionTextBox.SelectionLength;

            if (!DescriptionTextBox.SelectionFont.Style.ToString().Contains("Strikeout"))
                DescriptionTextBox.SelectionFont = new Font(DescriptionTextBox.Font, DescriptionTextBox.SelectionFont.Style | FontStyle.Strikeout);
            else
                DescriptionTextBox.SelectionFont = new Font(DescriptionTextBox.Font, DescriptionTextBox.SelectionFont.Style & ~FontStyle.Strikeout);

            DescriptionTextBox.Select(selStart, selLength);
        }

        private void UnderlineButton_Click(object sender, EventArgs e)
        {
            int selStart = DescriptionTextBox.SelectionStart;
            int selLength = DescriptionTextBox.SelectionLength;

            if (!DescriptionTextBox.SelectionFont.Style.ToString().Contains("Underline"))
                DescriptionTextBox.SelectionFont = new Font(DescriptionTextBox.Font, DescriptionTextBox.SelectionFont.Style | FontStyle.Underline);
            else
                DescriptionTextBox.SelectionFont = new Font(DescriptionTextBox.Font, DescriptionTextBox.SelectionFont.Style & ~FontStyle.Underline);

            DescriptionTextBox.Select(selStart, selLength);
        }

        async private void FindImageButton_Click(object sender, EventArgs e)
        {
            List<string> bolded = new List<string> { };
            List<string> thumbnails = new List<string> { };
            ImageURLs = new List<string> { };
            List<PictureBox> imageBoxes = new List<PictureBox> { pictureBox1, pictureBox2, pictureBox3, 
                pictureBox4, pictureBox5, pictureBox6, pictureBox7, pictureBox8, pictureBox9 };

            int index = 0;
            int wordCount = DescriptionTextBox.Text.Split(' ').Length;
            string[] words = DescriptionTextBox.Text.Split(' ');

            for (int i = 0; i < wordCount; i++)
            {
                DescriptionTextBox.Select(index, words[i].Length);
                string temp = DescriptionTextBox.SelectedText;

                if (DescriptionTextBox.SelectionFont.Style.ToString().Contains("Bold"))
                {
                    bolded.Add(words[i]);
                }

                index += 1 + words[i].Length;
            }

            var pexelsClient = new PexelsClient("563492ad6f917000010000013a071a50d88f43419b2550f2a08eaced");
            var result = await pexelsClient.SearchPhotosAsync(TitleTextBox.Text, "", "", "", "", 1, 5);

            for(int i = 0; i < result.photos.Count; i++)
            {
                thumbnails.Add(result.photos[i].source.tiny);
                ImageURLs.Add(result.photos[i].source.original);
            }
            
            foreach (string word in bolded)
            {
                result = await pexelsClient.SearchPhotosAsync(word, "", "", "", "", 1, 5);

                for (int i = 0; i < result.photos.Count; i++)
                {
                    thumbnails.Add(result.photos[i].source.tiny);
                    ImageURLs.Add(result.photos[i].source.original);
                }
            }

            List<int> indexes = Enumerable.Range(0, thumbnails.Count()).ToList();
            Random rand = new Random();
            indexes = indexes.OrderBy(a => rand.Next()).ToList();
            List<string> newThumbnails = indexes.Select(a => thumbnails[a]).ToList();
            List<string> newImageURLs = indexes.Select(b => ImageURLs[b]).ToList();
            ImageURLs = newImageURLs;

            foreach (PictureBox box in imageBoxes)
            {
                box.Load(newThumbnails[imageBoxes.IndexOf(box)]);
            }
        }

        private async void GenerateButton_Click(object sender, EventArgs e)
        {
            List<CheckBox> checkBoxes = new List<CheckBox> { checkBox1, checkBox2, checkBox3, checkBox4, 
                checkBox5, checkBox6, checkBox7, checkBox8, checkBox9 };
            string folder = @"Resources\" + DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss");

            foreach (CheckBox box in checkBoxes)
            {
                int i = checkBoxes.IndexOf(box);
                if (box.Checked)
                {
                    await DownloadImageAsync(folder, "image" + i, new Uri(ImageURLs[i], UriKind.Absolute));
                }
            }

            GenerateSlide(folder);
        }

        private async Task DownloadImageAsync(string directoryPath, string fileName, Uri uri)
        {
            var httpClient = new HttpClient();

            // Get the file extension
            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            // Create file path and ensure directory exists
            var path = Path.Combine(Application.StartupPath, directoryPath, $"{fileName}{fileExtension}");
            Directory.CreateDirectory(directoryPath);

            // Download the image and write to the file
            var imageBytes = await httpClient.GetByteArrayAsync(uri);
            File.WriteAllBytes(path, imageBytes);
        }

        private void GenerateSlide(string folder)
        {
            using (Presentation presentation = new Presentation())
            {
                ISlide slide = presentation.Slides[0];

                //Add content
                IAutoShape shape = slide.Shapes.AddAutoShape(ShapeType.Rectangle, 300, 75, 300, 50);
                shape.AddTextFrame(TitleTextBox.Text);
                ITextFrame textFrame = shape.TextFrame;
                IParagraph desc = textFrame.Paragraphs[0];
                IPortion portion = desc.Portions[0];
                desc.Text = DescriptionTextBox.Text;

                //var svgImg = presentation.Images.Add

                //presentation.Slides[0].Shapes.AddPictureFrame(ShapeType.Rectangle, 0, 0, 100, 100, );
                


                presentation.Save(TitleTextBox.Text + ".pptx", Aspose.Slides.Export.SaveFormat.Pptx);
            }

            ImageURLs = new List<string> { };
        }
    }
}