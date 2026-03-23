using System.Collections.Generic;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Models
{
    public class TaskMonitoringViewModel
    {
        public TaskAssignment NewTask { get; set; } = new TaskAssignment();
        public List<TaskAssignment> Tasks { get; set; } = new List<TaskAssignment>();
        public List<User> BHWs { get; set; } = new List<User>();
        public List<Household> Households { get; set; } = new List<Household>();
    }
}