#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProjectPorcupine.OrderActions
{
    [Serializable]
    [XmlRoot("OrderAction")]
    [OrderActionName("Deconstruct")]
    public class Deconstruct : OrderAction
    {
        public Deconstruct()
        {
        }

        private Deconstruct(Deconstruct other) : base(other)
        {
            JobInfo = other.JobInfo;
            Inventory = other.Inventory;
        }

        [XmlElement("Job")]
        public JobInformation JobInfo { get; set; }

        [XmlElement("Inventory")]
        public List<InventoryInfo> Inventory { get; set; }

        public override void Initialize(string type)
        {
            base.Initialize(type);

            // if there is no JobInfo defined, use defaults (time=0, ...)
            if (JobInfo == null)
            {
                JobInfo = new JobInformation();
            }
        }

        public override OrderAction Clone()
        {
            return new Deconstruct(this);
        }

        public override Job CreateJob(Tile tile, string type)
        {
            Job job = CheckJobFromFunction(JobInfo.FromFunction, tile.Furniture);

            if (job == null)
            {
                job = new Job(
                tile,
                type,
                null,
                JobInfo.Time,
                null,
                Job.JobPriority.High);
                job.Description = "job_deconstruct_" + type + "_desc";
                job.adjacent = true;
                job.OrderName = Type;
            }

            return job;
        }
    }
}
