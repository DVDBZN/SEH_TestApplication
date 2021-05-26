﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
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
        public List<string> ImagePaths { get; set; }

        public PowerPointCreator()
        {
            InitializeComponent();

            ImageURLs = new List<string> { };
            ImagePaths = new List<string> { };
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
            List<PictureBox> imageBoxes = new List<PictureBox> { pictureBox1, pictureBox2, pictureBox3,
                pictureBox4, pictureBox5, pictureBox6, pictureBox7, pictureBox8, pictureBox9 };

            int index = 0;
            int wordCount = DescriptionTextBox.Text.Split(' ').Length;
            string[] words = DescriptionTextBox.Text.Split(' ');

            //Iterates through all words and adds bolded ones to list
            for (int i = 0; i < wordCount; i++)
            {
                DescriptionTextBox.Select(index, words[i].Length);
                string temp = DescriptionTextBox.SelectedText;

                if (DescriptionTextBox.SelectionFont.Style.ToString().Contains("Bold"))
                    bolded.Add(words[i]);

                index += 1 + words[i].Length;
            }

            //Allows API calls
            var pexelsClient = new PexelsClient("563492ad6f917000010000013a071a50d88f43419b2550f2a08eaced");

            try
            {
                //Fetches 9 images based on title
                var result = await pexelsClient.SearchPhotosAsync(TitleTextBox.Text, "", "", "", "", 1, 9);

                for (int i = 0; i < result.photos.Count; i++)
                {
                    //Adds image URLs to lists
                    thumbnails.Add(result.photos[i].source.tiny);
                    ImageURLs.Add(result.photos[i].source.original);
                }

                foreach (string word in bolded)
                {
                    //Fetches 5 images for each bolded word and adds their URLs to lists
                    result = await pexelsClient.SearchPhotosAsync(word, "", "", "", "", 1, 5);

                    for (int i = 0; i < result.photos.Count; i++)
                    {
                        thumbnails.Add(result.photos[i].source.tiny);
                        ImageURLs.Add(result.photos[i].source.original);
                    }
                }
            }
            //In case text is not entered
            catch
            {
                MessageBox.Show("Please enter a title and description.", "Missing text", MessageBoxButtons.OK);
                return;
            }

            //Shuffles both lists identically
            List<int> indexes = Enumerable.Range(0, thumbnails.Count()).ToList();
            Random rand = new Random();
            indexes = indexes.OrderBy(a => rand.Next()).ToList();
            List<string> newThumbnails = indexes.Select(a => thumbnails[a]).ToList();
            List<string> newImageURLs = indexes.Select(b => ImageURLs[b]).ToList();
            ImageURLs = newImageURLs;

            //Loads 9 "random" images for display
            foreach (PictureBox box in imageBoxes)
                box.Load(newThumbnails[imageBoxes.IndexOf(box)]);
        }

        private async void GenerateButton_Click(object sender, EventArgs e)
        {
            List<CheckBox> checkBoxes = new List<CheckBox> { checkBox1, checkBox2, checkBox3, checkBox4,
                checkBox5, checkBox6, checkBox7, checkBox8, checkBox9 };
            //Name for folder for downloaded images
            string folder = @"Resources\" + DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss");

            foreach (CheckBox box in checkBoxes)
            {
                int i = checkBoxes.IndexOf(box);
                //Downloads checked images
                if (box.Checked)
                    await DownloadImageAsync(folder, "image" + i, new Uri(ImageURLs[i], UriKind.Absolute));
            }

            GenerateSlide();
        }

        private async Task DownloadImageAsync(string directoryPath, string fileName, Uri uri)
        {
            var httpClient = new HttpClient();

            //Get the file extension
            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            //Create file path and ensure directory exists
            var path = Path.Combine(Application.StartupPath, directoryPath, $"{fileName}{fileExtension}");
            Directory.CreateDirectory(directoryPath);

            //Download the image and write to the file
            var imageBytes = await httpClient.GetByteArrayAsync(uri);
            File.WriteAllBytes(path, imageBytes);

            ImagePaths.Add(path);
        }

        private void GenerateSlide()
        {
            //Create presentation file and slide
            Presentation presentation = new Presentation();
            ISlide slide = presentation.Slides[0];

            //Add content
            try
            {
                //Add all downloaded images to presentation file
                foreach (string path in ImagePaths)
                {
                    int i = ImagePaths.IndexOf(path);

                    FileStream strm = new FileStream(path, FileMode.Open);

                    IPPImage img = presentation.Images.AddImage(strm, LoadingStreamBehavior.KeepLocked);

                    presentation.Slides[0].Shapes.AddPictureFrame(ShapeType.Rectangle, (400 * (i + 1)) / ImagePaths.Count, (300 * (i + 1)) / ImagePaths.Count, 300, 200, img);
                }
            }
            //This may be unnecessary after recent bug fix
            catch
            {
                return;
            }

            //Create and format Title and Description texts
            IAutoShape shape = slide.Shapes.AddAutoShape(ShapeType.Rectangle, 250, 75, 300, 50);
            shape.ShapeStyle.LineColor.Color = Color.FromArgb(255, 255, 255, 255);
            shape.ShapeStyle.FillColor.Color = Color.FromArgb(200, 255, 255, 255);
            shape.ShapeStyle.FontColor.Color = Color.FromArgb(255, 0, 0, 0);
            shape.AddTextFrame("");
            ITextFrame textFrame = shape.TextFrame;
            IParagraph para = textFrame.Paragraphs[0];
            IPortion portion = para.Portions[0];
            portion.Text = TitleTextBox.Text;

            IAutoShape shape2 = slide.Shapes.AddAutoShape(ShapeType.Rectangle, 100, 150, 550, 375);
            shape2.ShapeStyle.LineColor.Color = Color.FromArgb(255, 255, 255, 255);
            shape2.ShapeStyle.FillColor.Color = Color.FromArgb(200, 255, 255, 255);
            shape2.ShapeStyle.FontColor.Color = Color.FromArgb(255, 0, 0, 0);
            shape2.AddTextFrame("");
            ITextFrame textFrame2 = shape2.TextFrame;
            IParagraph para2 = textFrame2.Paragraphs[0];
            IPortion portion2 = para2.Portions[0];
            portion2.Text = DescriptionTextBox.Text;

            //Save presentation file
            presentation.Save(TitleTextBox.Text + ".pptx", Aspose.Slides.Export.SaveFormat.Pptx);

            //Reset lists
            ImageURLs = new List<string> { };
            ImagePaths = new List<string> { };

            //Confirmation
            MessageBox.Show("File located at " + Path.Combine(Application.StartupPath, TitleTextBox.Text + ".pptx"), "Slide Created!", MessageBoxButtons.OK);
        }
    }
}