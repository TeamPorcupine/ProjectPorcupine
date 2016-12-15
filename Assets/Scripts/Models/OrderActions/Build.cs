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
using System.Linq;
using System.Xml.Serialization;
using ProjectPorcupine.Jobs;

namespace ProjectPorcupine.OrderActions
{
    [Serializable]
    [XmlRoot("OrderAction")]
    [OrderActionName("Build")]
    public class Build : OrderAction
    {
        public Build()
        {
        }

        private Build(Build other) : base(other)
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
            return new Build(this);
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
                Inventory.Select(it => new RequestedItem(it.Type, it.Amount)).ToArray(),
                Job.JobPriority.High);
                job.Description = "job_build_" + type + "_desc";
                job.OrderName = Type;
            }

            return job;
        }
    }
}
