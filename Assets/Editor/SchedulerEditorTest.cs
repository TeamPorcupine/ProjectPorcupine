#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using NUnit.Framework;
using Scheduler;

public class SchedulerEditorTest
{
    private Scheduler.Scheduler scheduler;
    private Action<ScheduledEvent> callback;

    [SetUp]
    public void Init()
    {
        // The problem with unit testing singletons
        ///scheduler = Scheduler.Scheduler.Current;
        scheduler = new Scheduler.Scheduler();

        callback = (e) => Debug.ULogChannel("SchedulerTest", "Event {0} fired", e.Name);
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
        Assert.That(scheduler.IsRegistered(evt), Is.True);
        Assert.That(scheduler.Events.Count, Is.EqualTo(1));
        scheduler.RegisterEvent(evt);
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
        Assert.That(scheduler.IsRegistered(evt), Is.True);
        Assert.That(scheduler.Events.Count, Is.EqualTo(1));

        scheduler.DeregisterEvent(evt);
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
            (e) => { tally++; callback(e); },
            2.0f,
            true,
            1);
        ScheduledEvent evt2 = new ScheduledEvent(
            "test - increment i by 10",
            (e) => { tally += 10; callback(e); },
            3.0f,
            true,
            1);

        scheduler.RegisterEvent(evt1);
        scheduler.RegisterEvent(evt2);

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
        Assert.That(scheduler.TimeToNextEvent(), Is.EqualTo(3));
        scheduler.RegisterEvent(evt1);
        Assert.That(scheduler.TimeToNextEvent(), Is.EqualTo(2));
    }

    [Test]
    public void SchedulerPurgeTest()
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

        evt2.Stop();
        Assert.That(scheduler.IsRegistered(evt2), Is.True);
        scheduler.PurgeEventList();
        Assert.That(scheduler.IsRegistered(evt2), Is.False);

        evt1.Stop();
        Assert.That(scheduler.IsRegistered(evt1), Is.True);
        scheduler.PurgeEventList();
        Assert.That(scheduler.IsRegistered(evt1), Is.False);
    }
}
