using System;
using System.Threading.Tasks;
using System.Windows;

namespace TESV_EspEquipmentGenerator
{
    /// <summary>
    /// Interaction logic for PupopProgress.xaml
    /// </summary>
    public partial class PupopProgress : Window
    {
        public int from = 0;
        public int to = 100;

        public void MoveNextStep(int amount) {
            from = (int)Value;
            to = from + amount; 
        }

        public string ProgressText = "Progressing";
        public double normalizeValue {
            get => progressbar.Value*0.01;
            set {
                progressbar.Value = from+ ((to-from)* value);
                Title = ProgressText + "....." + Value.ToString() + "%";
            } 
        }
        public double Value
        {
            get => progressbar.Value;
            set => progressbar.Value = value;
        }

        public PupopProgress()
        {
            InitializeComponent();
            Confirm_btn.Click += (s, e) => Close();
        }
        public static void Start(Action<PupopProgress> work) {
            PupopProgress pupopProgress = new PupopProgress();
            pupopProgress.Show();
            Task.Run(() =>
            {
                work(pupopProgress);
            });
        }
    }
}
