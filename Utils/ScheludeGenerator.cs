using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorScheludeApp.Utils
{
    class ScheludeGenerator
    {
        private DateTime _startDate;
        private DateTime _endDate;
        private List<DataBase.DoctorSchedule> _doctorSchedule;

        public ScheludeGenerator(DateTime startDate, DateTime endDate, List<DataBase.DoctorSchedule> schedule)
        {
            _startDate = startDate;
            _endDate = endDate;
            _doctorSchedule = schedule.Where(p => p.Date >= _startDate.Date && p.Date <= _endDate.Date).ToList();
        }

        public List<DataBase.ScheduleHeader> GenerateHeaders()
        {
            var result = new List<DataBase.ScheduleHeader>();

            var startDate = _startDate;
            while(startDate.Date < _endDate.Date)
            {
                result.Add(new DataBase.ScheduleHeader
                {
                    Date = startDate.Date
                });
                startDate = startDate.AddDays(1);
            }

            return result;
        }

        public List<List<DataBase.ScheduleAppointment>> GenerateAppointments(List<DataBase.ScheduleHeader> headers)
        {
            var result = new List<List<DataBase.ScheduleAppointment>>();

            if(_doctorSchedule.Count() > 0)
            {
                var minStartTime = _doctorSchedule.Min(p => p.StartTime);
                var maxEndTime = _doctorSchedule.Max(p => p.EndTime);

                var startTime = minStartTime;
                while(startTime < maxEndTime)
                {
                    // Формирование ячеек по горизонтали по текущему интервалу на каждый день
                    var appointmentsPerInterval = new List<DataBase.ScheduleAppointment>();

                    //Перебор каждого дня по указанному выше интервалу
                    foreach(var header in headers)
                    {
                        var currentSchedule = _doctorSchedule.FirstOrDefault(p => p.Date == header.Date);

                        var scheduleAppointment = new DataBase.ScheduleAppointment
                        {
                            ScheduleId = currentSchedule?.Id ?? -1,
                            StartTime = startTime,
                            EndTime = startTime.Add(TimeSpan.FromMinutes(30))
                        };
                        // Определение типа ячейки для записи
                        // Если запись есть - врач работает, проверяем, есть ли запись на этот интервал
                        if(currentSchedule != null)
                        {
                            var busyAppointment = currentSchedule.Appointment.FirstOrDefault(p => p.StartTime == startTime);

                            //Если запись уже есть 
                            if(busyAppointment != null)
                            {
                                scheduleAppointment.AppointmentType = DataBase.AppointmentType.Busy;
                            }
                            // Иначе ячейка свободна ( запись возможна) 
                            else
                            {
                                //Проверяем, начался ли рабочий день врача
                                if(startTime < currentSchedule.StartTime || startTime >= currentSchedule.EndTime)
                                {
                                    //Рабочий день еще не чачат или уже закончен
                                    scheduleAppointment.AppointmentType = DataBase.AppointmentType.DayOff;
                                }
                                else
                                {
                                    //Запись возможна 
                                    scheduleAppointment.AppointmentType = DataBase.AppointmentType.Free;
                                }
                            }
                        }
                        else
                        {
                            // Врач не работает 
                            scheduleAppointment.AppointmentType = DataBase.AppointmentType.DayOff;
                        }

                        appointmentsPerInterval.Add(scheduleAppointment);
                    }

                    result.Add(appointmentsPerInterval);
                    startTime = startTime.Add(TimeSpan.FromMinutes(30)); 
                }
            }

            return result;
        }
    }
}
