﻿using System;
using System.Collections.Generic;
using TimeTrackerTutorial.Services;

namespace TimeTrackerTutorial.Models
{
    public class TestData : IIdentifiable
    {
        public string Id { get; set; }
        public int Age { get; set; }
        public double Amount { get; set; }
        public bool Flag { get; set; }
        public string Name { get; set; }
        public DateTime SomeDate { get; set; }

        // Test fields, testing arrays and maps
        public List<WorkItem> Work { get; set; }
    }
}
