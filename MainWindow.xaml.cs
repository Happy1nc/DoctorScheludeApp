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

namespace DoctorScheludeApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ComboSpecialisation.ItemsSource = App.DataBase.Specialization.ToList();
            ComboSpecialisation.SelectedIndex = 0;
        }

        private void ComboSpecialisation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedSpecialisation = ComboSpecialisation.SelectedItem as DataBase.Specialization;
            if (selectedSpecialisation != null)
            {
                var doctors = App.DataBase.Doctor.ToList()
                    .Where(p => p.Specialization == selectedSpecialisation);
                ComboDoctor.ItemsSource = doctors;
                ComboDoctor.SelectedIndex = 0; 
            }
        }

        private void ComboDoctor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDoctor = ComboDoctor.SelectedItem as DataBase.Doctor;
            if(selectedDoctor != null)
            {
                GenerateSchedule(selectedDoctor);
            }
        }
        private void GenerateSchedule(DataBase.Doctor SelectedDoctor)
        {
            var startDate = DateTime.Now.Date;
            var endDate = startDate.AddDays(5);

            var scheduleGenerator = new Utils.ScheludeGenerator(startDate, endDate, SelectedDoctor.DoctorSchedule.ToList());

            var headers = scheduleGenerator.GenerateHeaders();

            var appointments = scheduleGenerator.GenerateAppointments(headers);
            LoadSchedule(headers, appointments);
        }
        private void LoadSchedule(List<DataBase.ScheduleHeader> headers, List<List<DataBase.ScheduleAppointment>> appointments)
        {
            DGridSchedule.Columns.Clear();
            for (int i = 0; i < headers.Count(); i++)
            {
                //Фабрика для настройки заголовков 
                FrameworkElementFactory headerFactory = new FrameworkElementFactory(typeof(UserControls.ScheludeHeaderControl));
                headerFactory.SetValue(DataContextProperty, headers[i]);
                
                //Шаблон заголовка
                var headerTemplate = new DataTemplate
                {
                    VisualTree = headerFactory
                };

                //Фабрика для настройки ячейки
                FrameworkElementFactory cellFactory = new FrameworkElementFactory(typeof(UserControls.ScheduleAppointmentControl));
                
                //Привязка текущего столбца к индексу списка записей
                cellFactory.SetBinding(DataContextProperty, new Binding($"[{i}]"));
                //Обратотка событий 
                cellFactory.AddHandler(MouseDownEvent, new MouseButtonEventHandler(BtnAppointment_MouseDown), true);


                //Шаблон ячейки
                var cellTemplate = new DataTemplate
                {
                    VisualTree = cellFactory
                };

                //Создание нового столбца с указанием шаблона заголовка, размера ячейки, заголовка
                var columnTemplate = new DataGridTemplateColumn
                {
                    HeaderTemplate = headerTemplate,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTemplate = cellTemplate
                };
                // Добавление столца в таблицу 
                DGridSchedule.Columns.Add(columnTemplate);
            }

                DGridSchedule.ItemsSource = appointments;
        }

        private void BtnAppointment_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentControl = sender as UserControls.ScheduleAppointmentControl;
            var currentAppointment = currentControl.DataContext as DataBase.ScheduleAppointment;

            if(currentAppointment != null && currentAppointment.ScheduleId > 0 && currentAppointment.AppointmentType == DataBase.AppointmentType.Free)
            {
                App.DataBase.Appointment.Add(new DataBase.Appointment
                {
                    DoctorScheduleId = currentAppointment.ScheduleId,
                    StartTime = currentAppointment.StartTime,
                    EndTime = currentAppointment.EndTime,
                    ClientId = 1
                });

                App.DataBase.SaveChanges();
                MessageBox.Show("Вы записаны на прием!");
                ComboDoctor_SelectionChanged(ComboDoctor, null);
            }
        }
    }
}
