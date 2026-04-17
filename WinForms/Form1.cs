using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using static lab1_wf.Form1;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace lab1_wf
{
    public partial class Form1 : Form
    {

        private List<Question> _questions;
        private List<string> _answers = new List<string>();
        private bool _started = false;
        private int _index = 0;
        private int size = 0;
        private string username;
        private string title;

        public class Question
        {
            public int Number { get; set; }
            public string Text { get; set; }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = !string.IsNullOrWhiteSpace(textBox1.Text);
        }


        public void checkName()
        {
            label1.Text = title;			// назва форми опитування.
            label2.Text = "Введіть ім'я";	// текст питання.
            textBox1.Text = "";				// очищення текстового поля.
            button1.Text = "Почати";			// зміна тексту кнопки.
        }

        public void StartSurvey(List<Question> list) // початок опитування.
        {
            label1.Anchor = AnchorStyles.Left;
            label2.Anchor = AnchorStyles.Left;
            label1.Margin = new Padding(15, 10, 0, 10);
            label2.Margin = new Padding(20, 0, 0, 10);

            textBox1.Margin = new Padding(25, 0, 0, 10);
            textBox1.AcceptsReturn = true;
            textBox1.WordWrap = true;
            textBox1.Anchor = AnchorStyles.Left;
            textBox1.Multiline = true;
            textBox1.Dock = DockStyle.None;

            tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Absolute;
            tableLayoutPanel1.RowStyles[2].Height = 80;
            textBox1.Height = 80;
            button1.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            button1.Margin = new Padding(25, 0, 0, 10);

            _index = 0;
            ShowQuestion();	// показ питань.
        }

        private void ShowQuestion()
        {
            if (_index >= size)	// перевірка на кінець опитування
            {
                textBox1.Visible = false;
                tableLayoutPanel1.RowStyles[2].Height = 0;
                label1.Visible = false;
                label2.Text = "Готово!";
                label2.Font = new Font(textBox1.Font.FontFamily, 24);
                label2.Anchor = AnchorStyles.None;
                label2.Margin = new Padding(5, 100, 0, 0);
                button1.Text = "Вийти";
                button1.Margin = new Padding(0, 10, 0, 0);
                button1.Anchor = AnchorStyles.Top;
                return;
            }

            var q = _questions[_index];	// отримання питання за індексом

            label1.Text = $"Питання {q.Number}/{size}"; // номер питання.
            label2.Text = q.Text;     // текст питання.
            textBox1.Text = "";
            button1.Text = "Далі";
        }

        private void SaveAnswers()	// зберігання відповідей у файл.
        {
            string folder = "Answers";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            //перевірка існуючих відповідей
            var files = Directory.GetFiles(folder, "Answers*.xml");

            int maxIndex = 0;
            // цикл для знаходження останнього індексу Answers{n}.xml
            foreach (var file in files)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(file); // Answer
                string numberPart = new string(name.Where(char.IsDigit).ToArray());

                if (int.TryParse(numberPart, out int n))
                {
                    if (n > maxIndex)
                        maxIndex = n;
                }
            }


            XDocument doc = new XDocument(
                new XElement("Results",
                    new XAttribute("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    new XAttribute("username", username),

                    _answers.Select((a, i) =>
                        new XElement("Answer",
                            new XAttribute("Question", _questions[i].Text),
                            a
                        )
                    )
                )
            );

            doc.Save($"Answers/Answers{maxIndex + 1}.xml");
        }


        public Form1()
        {
            InitializeComponent();
            textBox1.TextChanged += textBox1_TextChanged;
            this.BackColor = ColorTranslator.FromHtml("#BFBCCB");
            tableLayoutPanel1.BackColor = ColorTranslator.FromHtml("#F4F3F6");

            string path = "Forms/Form1.xml";
            // читання xml в title та questions.
            try
            {
                XDocument doc = XDocument.Load(path);
                title = (string)doc.Root.Attribute("title");
                List<Question> questions = doc.Root
                    .Elements("Question")
                    .Select(q => new Question
                    {
                        Number = (int)q.Attribute("number"),
                        Text = (string)q.Element("Text")
                    })
                    .ToList();
                _questions = questions;
                size = _questions.Count;	// кількість питань
                checkName();		// перейти до запиту імені.
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні файлу: {ex.Message}");
                //Application.Current.Shutdown();
                return;
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            if (!_started)	// якщо опитування не почалося, записати ім’я та почати.
            {
                username = textBox1.Text;
                _started = true;
                StartSurvey(_questions);
                return;
            }
            // якщо опитування завершено, зберегти та вийти.
            if (_index >= _questions.Count)
            {
                SaveAnswers();
                this.Close();
                return;
            }
            // запис відповідей у змінну для подальшого запису в файл.
            _answers.Add(textBox1.Text);
            _index++;
            ShowQuestion();
        }
    }
}
