#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Scheduler;

public class SchedulerEditorTest
{
    private const string LuaFunctionString = @"
        function ping_log_lua(event)
            ModUtils.ULogChannel(""Scheduler"", ""Scheduled Lua event '"" .. event.Name .. ""'"")
            return
        end";
    
    private const string XmlPrototypeString = @"
        <ScheduledEvents>
            <ScheduledEvent name=""ping_log_lua"" onFire=""ping_log_lua""/>
        </ScheduledEvents>";

    private Scheduler.Scheduler scheduler;
    private Action<ScheduledEvent> callback;

    [SetUp]
    public void Init()
    {
        Debug.IsLogEnabled = false;

        if (FunctionsManager.ScheduledEvent == null)
        {
            new FunctionsManager();
        }

        FunctionsManager.ScheduledEvent.LoadScript(LuaFunctionString, "ScheduledEvent");

        if (PrototypeManager.ScheduledEvent == null)
        {
            new PrototypeManager();
        }

        PrototypeManager.ScheduledEvent.Add(new ScheduledEvent("ping_log", evt => Debug.ULogChannel("Scheduler", "Event {0} fired", evt.Name)));
        PrototypeManager.ScheduledEvent.LoadPrototypes(XmlPrototypeString);

        // The problem with unit testing singletons
        ///scheduler = Scheduler.Scheduler.Current;
        scheduler = new Scheduler.Scheduler();

        callback = evt => Debug.ULogChannel("SchedulerTest", "Event {0} fired", evt.Name);
    }

    [Test]
    public void SchedulerRegistrationTest()
    {
        Assert.That(scheduler.Events.Count, Is.EqualTo(0));

        ScheduledEvent evt = new ScheduledEvent(
            "test",
            callback,
            3.0f,
            true,
            1);

        Assert.That(scheduler.IsRegistered(evt), Is.False);

        scheduler.RegisterEvent(evt);
        scheduler.Update(0); // updates the event list
        Assert.That(scheduler.IsRegistered(evt), Is.True);
        Assert.That(scheduler.Events.Count, Is.EqualTo(1));

        scheduler.RegisterEvent(evt);
        scheduler.Update(0); // updates the event list
        Assert.That(scheduler.IsRegistered(evt), Is.True);
        Assert.That(scheduler.Events.Count, Is.EqualTo(2));
    }

    [Test]
    public void SchedulerDeregistrationTest()
    {
        Assert.That(scheduler.Events.Count, Is.EqualTo(0));

        ScheduledEvent evt = new ScheduledEvent(
            "test",
            callback,
            3.0f,
            true,
            1);

        Assert.That(scheduler.IsRegistered(evt), Is.False);
        scheduler.RegisterEvent(evt);
        scheduler.Update(0); // updates the event list
        Assert.That(scheduler.IsRegistered(evt), Is.True);
        Assert.That(scheduler.Events.Count, Is.EqualTo(1));

        scheduler.DeregisterEvent(evt);
        scheduler.Update(0); // updates the event list
        Assert.That(scheduler.IsRegistered(evt), Is.False);
        Assert.That(scheduler.Events.Count, Is.EqualTo(0));
    }

    [Test]
    public void SchedulerRunTest()
    {
        // a tally value which counts event firings
        int tally = 0;
        Assert.That(scheduler.Events.Count, Is.EqualTo(0));

        ScheduledEvent evt1 = new ScheduledEvent(
            "test - increment i by 1",
            evt => { tally++; callback(evt); },
            2.0f,
            true,
            1);
        ScheduledEvent evt2 = new ScheduledEvent(
            "test - increment i by 10",
            evt => { tally += 10; callback(evt); },
            3.0f,
            true,
            1);

        scheduler.RegisterEvent(evt1);
        scheduler.RegisterEvent(evt2);
        scheduler.Update(0); // updates the event list

        // at times 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 respectively
        int[] expectedTally = { 0, 0, 1, 11, 12, 12, 23, 23, 24, 34, 35 };
        for (int t = 0; t <= 10; ++t)
        {
            Assert.That(tally, Is.EqualTo(expectedTally[t]));
            scheduler.Update(1.0f);
        }
    }

    [Test]
    public void SchedulerTimeToNextEventTest()
    {
        Assert.That(scheduler.Events.Count, Is.EqualTo(0));

        ScheduledEvent evt1 = new ScheduledEvent(
            "test",
            callback,
            2.0f,
            true,
            1);
        ScheduledEvent evt2 = new ScheduledEvent(
            "test",
            callback,
            3.0f,
            true,
            1);

        scheduler.RegisterEvent(evt2);
        scheduler.Update(0); // updates the event list
        Assert.That(scheduler.TimeToNextEvent(), Is.EqualTo(3));
        scheduler.RegisterEvent(evt1);
        scheduler.Update(0); // updates the event list
        Assert.That(scheduler.TimeToNextEvent(), Is.EqualTo(2));
    }

    [Test]
    public void SchedulerClearFinishedEventsTest()
    {
        Assert.That(scheduler.Events.Count, Is.EqualTo(0));

        ScheduledEvent evt1 = new ScheduledEvent(
            "test",
            callback,
            2.0f,
            true,
            1);
        ScheduledEvent evt2 = new ScheduledEvent(
            "test",
            callback,
            3.0f,
            true,
            1);

        scheduler.RegisterEvent(evt1);
        scheduler.RegisterEvent(evt2);
        scheduler.Update(0); // updates the event list

        evt2.Stop();
        Assert.That(scheduler.IsRegistered(evt2), Is.True);
        scheduler.Update(0); // updates the event list
        Assert.That(scheduler.IsRegistered(evt2), Is.False);

        evt1.Stop();
        Assert.That(scheduler.IsRegistered(evt1), Is.True);
        scheduler.Update(0); // updates the event list
        Assert.That(scheduler.IsRegistered(evt1), Is.False);
    }

    [Test]
    public void SchedulerLuaEventTest()
    {
        ScheduledEvent evt = new ScheduledEvent(PrototypeManager.ScheduledEvent.Get("ping_log_lua"), 1.0f, 1.0f, false, 1);

        scheduler.RegisterEvent(evt);

        scheduler.Update(1);
    }

    [Test]
    public void SchedulerEventsDoNotModifyEventsListDuringUpdateTest()
    {
        Assert.That(scheduler.Events.Count, Is.EqualTo(0));

        int numEventsInList = 0;

        // generic event that expires at time 2s
        ScheduledEvent evt1 = new ScheduledEvent(
            "test",
            callback,
            2.0f,
            false,
            1);

        // event which tries to purge the event list at 5s
        ScheduledEvent evt2 = new ScheduledEvent(
            "test",
            evt => scheduler.DeregisterEvent(evt1),
            5.0f,
            false,
            1);

        // event which counts the events in the list at 7s
        ScheduledEvent evt3 = new ScheduledEvent(
            "test",
            evt => numEventsInList = scheduler.Events.Count,
            7.0f,
            false,
            1);

        scheduler.RegisterEvent(evt1);
        scheduler.RegisterEvent(evt2);
        scheduler.RegisterEvent(evt3);
        scheduler.Update(10f);

        // evt2 does not remove evt1 from the list during the loop
        Assert.That(numEventsInList, Is.EqualTo(3));

        // but Update() correctly purges at the end of each call
        Assert.That(scheduler.Events.Count, Is.EqualTo(0));
    }

    [Test]
    public void SchedulerEventsListIsReadOnlyTest()
    {
        Assert.That(scheduler.Events.Count, Is.EqualTo(0));

        Assert.That(scheduler.Events, Is.TypeOf(typeof(ReadOnlyCollection<ScheduledEvent>)));
    }

    [Test]
    public void SchedulerWriteXmlTest()
    {
        scheduler.ScheduleEvent("ping_log", 1f, true, 0);
        scheduler.ScheduleEvent("ping_log_lua", 2f, false, 3);
        scheduler.Update(0); // updates the event list

        StringBuilder sb = new StringBuilder();
        XmlWriter writer = new XmlTextWriter(new StringWriter(sb));
        scheduler.WriteXml(writer);

        string expectedXml = "<Scheduler><Event name=\"ping_log\" cooldown=\"1\" timeToWait=\"1\" repeatsForever=\"True\" /><Event name=\"ping_log_lua\" cooldown=\"2\" timeToWait=\"2\" repeatsLeft=\"3\" /></Scheduler>";
        Assert.That(sb.ToString(), Is.EqualTo(expectedXml));
    }

    [Test]
    public void SchedulerWriteXmlDoesNotWriteNoSaveFlaggedEventsTest()
    {
        ScheduledEvent evt = new ScheduledEvent(
            "test",
            callback,
            2.0f,
            false,
            1);
        evt.IsSaveable = false;
        scheduler.RegisterEvent(evt);
        scheduler.Update(0); // updates the event list

        StringBuilder sb = new StringBuilder();
        XmlWriter writer = new XmlTextWriter(new StringWriter(sb));
        scheduler.WriteXml(writer);

        string expectedXml = "<Scheduler />";
        Assert.That(sb.ToString(), Is.EqualTo(expectedXml));
    }

    [Test]
    public void SchedulerReadXmlTest()
    {
        // prepare a scheduler instance to serialize
        scheduler.ScheduleEvent("ping_log", 1f, true, 0);
        scheduler.ScheduleEvent("ping_log_lua", 2f, false, 3);
        scheduler.Update(0.5f);

        // serialize it
        StringBuilder sb = new StringBuilder();
        XmlWriter writer = new XmlTextWriter(new StringWriter(sb));
        scheduler.WriteXml(writer);
        XmlReader reader = new XmlTextReader(new StringReader(sb.ToString()));

        // Tt is not actually necessary to replace the scheduler: it will properly clear the old one.
        // This just proves that it does work to recreate the scheduler from nothing.
        scheduler = new Scheduler.Scheduler();
        scheduler.ReadXml(reader);

        Assert.That(scheduler.Events.Count, Is.EqualTo(2));
        Assert.That(scheduler.Events[0].Name, Is.EqualTo("ping_log"));
        Assert.That(scheduler.Events[0].Cooldown, Is.EqualTo(1));
        Assert.That(scheduler.Events[0].TimeToWait, Is.EqualTo(0.5f));
        Assert.That(scheduler.Events[0].RepeatsForever, Is.True);
        Assert.That(scheduler.Events[0].RepeatsLeft, Is.EqualTo(0));
        Assert.That(scheduler.Events[1].Name, Is.EqualTo("ping_log_lua"));
        Assert.That(scheduler.Events[1].Cooldown, Is.EqualTo(2));
        Assert.That(scheduler.Events[1].TimeToWait, Is.EqualTo(1.5f));
        Assert.That(scheduler.Events[1].RepeatsForever, Is.False);
        Assert.That(scheduler.Events[1].RepeatsLeft, Is.EqualTo(3));

        // Prove that it works even without creating a new scheduler instance.
        // First add an event so that the Asserts will fail if ReadXml does nothing.
        scheduler.ScheduleEvent("ping_log_lua", 5f, false, 20);
        scheduler.Update(0);

        reader = new XmlTextReader(new StringReader(sb.ToString()));
        scheduler.ReadXml(reader);

        Assert.That(scheduler.Events.Count, Is.EqualTo(2));
        Assert.That(scheduler.Events[0].Name, Is.EqualTo("ping_log"));
        Assert.That(scheduler.Events[0].Cooldown, Is.EqualTo(1));
        Assert.That(scheduler.Events[0].TimeToWait, Is.EqualTo(0.5f));
        Assert.That(scheduler.Events[0].RepeatsForever, Is.True);
        Assert.That(scheduler.Events[0].RepeatsLeft, Is.EqualTo(0));
        Assert.That(scheduler.Events[1].Name, Is.EqualTo("ping_log_lua"));
        Assert.That(scheduler.Events[1].Cooldown, Is.EqualTo(2));
        Assert.That(scheduler.Events[1].TimeToWait, Is.EqualTo(1.5f));
        Assert.That(scheduler.Events[1].RepeatsForever, Is.False);
        Assert.That(scheduler.Events[1].RepeatsLeft, Is.EqualTo(3));
    }
}
