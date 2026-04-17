using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Linq;


namespace lab1
{
    // Клас Question має властивості number та text - номер та текст питання.   
    public class Question
    {
        public int Number { get; set; }
        public string Text { get; set; }
    }  

    public partial class MainWindow : Window
    {
        // Оголошення змінних.
        private List<Question>_questions;
        private List <string>_answers = new List<string>();
        private bool _started = false;
        private int _index = 0;
        private int size = 0;
        private string username;
        private string title;
        public MainWindow()
        {
	// шлях до питань.
            string path = "Forms/Form1.xml";
	// читання xml в title та questions.
            try
            {
                XDocument doc = XDocument.Load(path);
                title = (string)doc.Root.Attribute("title");
                List <Question>questions = doc.Root
                    .Elements("Question")
                    .Select(q => new Question
                    {
                            Number = (int)q.Attribute("number"),
                            Text = (string)q.Element("Text")
                    })
                    .ToList();
                InitializeComponent();
                _questions = questions;
                size = _questions.Count;	// кількість питань
                checkName();		// перейти до запиту імені.
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні файлу: {ex.Message}");
                Application.Current.Shutdown();
                return;
            }
        }

        public void checkName()
        {
            Question_Name.Text = title;			// назва форми опитування.
            Question_Text.Text = "Введіть ім'я";	// текст питання.
            Answer.Text = "";				// очищення текстового поля.
            Submit.Content = "Почати";			// зміна тексту кнопки.
        }

        
         public void StartSurvey(List <Question>list) // початок опитування.
        {
            Question_Name.HorizontalAlignment = HorizontalAlignment.Left;
            Question_Text.HorizontalAlignment = HorizontalAlignment.Left;
            Question_Name.Margin = new Thickness(20, 0, 100, 10);
            Question_Text.Margin = new Thickness(20, 0, 100, 10);

            Answer.Margin = new Thickness(20, 0, 200, 10);
            Answer.AcceptsReturn = true;
            Answer.TextWrapping = TextWrapping.Wrap;
            AnswerRow.Height = new GridLength(80);

            Submit.HorizontalAlignment = HorizontalAlignment.Left;
            Submit.Margin = new Thickness(20, 0, 200, 10);
            
            _index = 0;     
            ShowQuestion();	// показ питань.
        }

        private void ShowQuestion()
        {
            if (_index >= size)	// перевірка на кінець опитування
            {
                Question_Name.Visibility = Visibility.Collapsed;
                Question_Text.Visibility = Visibility.Collapsed;
                Grid.SetRow(Answer, 0);
                Grid.SetRowSpan(Answer, 3);
                Answer.Text = "Готово!";
                Answer.Margin = new Thickness(100, 100, 100, 10);
                Answer.FontSize = 24;
                Answer.IsReadOnly = true;
                Answer.TextAlignment = TextAlignment.Center;
                Answer.VerticalAlignment = VerticalAlignment.Center;
                Answer.BorderThickness = new Thickness(0);           
                Submit.Content = "Вийти";
                Submit.Margin = new Thickness(0, 10, 0, 0);
                Submit.HorizontalAlignment = HorizontalAlignment.Center;
                return;
            }

            var q = _questions[_index];	// отримання питання за індексом

            Question_Name.Text = $"Питання {q.Number}/{size}"; // номер питання.
            Question_Text.Text = q.Text;     // текст питання.
            Answer.Text = "";
            Submit.Content = "Далі";
        }
        private void SaveAnswer()	// зберігання відповідей у файл.
        {
            string folder = "Answers";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
	
	//перевірка існуючих відповідей
            var files = Directory.GetFiles(folder, "answer*.xml");  

            int maxIndex = 0;
	// цикл для знаходження останнього індексу answer{n}.xml
            foreach (var file in files)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(file); // answer1
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
                            new XAttribute("Question", i + 1),
                            a
                        )
                    )
                )
            );

            doc.Save($"Answers/Answers{maxIndex + 1}.xml");
        }

        private void Button_Click(object sender, RoutedEventArgs e) // обробка кнопки.
        {
            if (!_started)	// якщо опитування не почалося, записати ім’я та почати.
            {
                username = Answer.Text;
                _started = true;
                StartSurvey(_questions);
                return;
            }         
	// якщо опитування завершено, зберегти та вийти.
            if (_index >= _questions.Count)  
            {
                SaveAnswer();
                Application.Current.Shutdown();
                return;
            }
	// запис відповідей у змінну для подальшого запису в файл.
            _answers.Add(Answer.Text);
            _index++;
            ShowQuestion();         
        }
    }
}
