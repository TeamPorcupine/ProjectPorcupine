#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Scheduler;

public class ScheduledEventTest
{
    private Action<ScheduledEvent> callback;
    private bool didRun = false;

    [SetUp]
    public void Init()
    {
        Debug.IsLogEnabled = false;
        callback = (evt) =>
            {
                Debug.ULogChannel("ScheduledEventTest", "Event {0} fired", evt.Name);
                didRun = true;
            };
    }

    [Test]
    public void EventCreationTest()
    {
        ScheduledEvent evt = new ScheduledEvent(
            "test",
            (ev) => Debug.ULogChannel("ScheduledEventTest", "Event {0} fired", ev.Name),
            3.0f,
            true,
            1);

        ScheduledEvent evt2 = new ScheduledEvent(evt);

        Assert.That(evt.Name, Is.EqualTo("test"));
        Assert.That(evt2.Name, Is.EqualTo("test"));
        Assert.That(evt, Is.Not.EqualTo(evt2));

        ScheduledEvent evt3 = new ScheduledEvent(
            new ScheduledEvent("test", (ev) => Debug.ULogChannel("ScheduledEventTest", "Event {0} fired", ev.Name)),
            1.0f,
            0.5f,
            false,
            3);

        Assert.That(evt3.Name, Is.EqualTo("test"));
        Assert.That(evt3.Cooldown, Is.EqualTo(1));
        Assert.That(evt3.TimeToWait, Is.EqualTo(0.5f));
        Assert.That(evt3.RepeatsForever, Is.False);
        Assert.That(evt3.RepeatsLeft, Is.EqualTo(3));
    }

    [Test]
    public void EndlessEventRunTest()
    {
        // event that repeats forever
        didRun = false;
        ScheduledEvent evt = new ScheduledEvent(
                                 "test1",
                                 callback,
                                 3.0f,
                                 true,
                                 1);
        evt.Fire();
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);
        Assert.That(didRun, Is.True);

        // event that repeats forever -- confirm that repeats is ignored
        didRun = false;
        evt = new ScheduledEvent(
            "test2",
            callback,
            3.0f,
            true,
            0);
        evt.Fire();
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);
        Assert.That(didRun, Is.True);
    }

    [Test]
    public void NTimesRepeatedEventRunTest()
    {
        // event that repeats twice
        didRun = false;
        ScheduledEvent evt = new ScheduledEvent(
            "test",
            callback,
            3.0f,
            false,
            2);
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);

        evt.Fire();
        Assert.That(didRun, Is.True);
        Assert.That(evt.LastShot, Is.True);
        Assert.That(evt.Finished, Is.False);
        didRun = false;

        evt.Fire();
        Assert.That(didRun, Is.True);
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.True);

        didRun = false;
        evt.Fire();
        Assert.That(evt.LastShot, Is.False);
        Assert.That(didRun, Is.False);
    }

    [Test]
    public void CooldownEventRunTest()
    {
        ScheduledEvent evt = new ScheduledEvent(
            "test",
            callback,
            3.0f,
            true,
            1);

        // doesn't run until the cooldown is reached
        didRun = false;
        evt.Update(2.0f);
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);
        Assert.That(evt.TimeToWait, Is.EqualTo(1.0f));
        Assert.That(didRun, Is.False);

        didRun = false;
        evt.Update(1.0f);
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);
        Assert.That(didRun, Is.True);

        // timer should be reset after firing
        Assert.That(evt.TimeToWait, Is.EqualTo(evt.Cooldown));

        // so it should work again as above
        didRun = false;
        evt.Update(2.0f);
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);
        Assert.That(didRun, Is.False);

        didRun = false;
        evt.Update(1.0f);
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);
        Assert.That(didRun, Is.True);

        // and it should also work if we overshoot the cooldown
        didRun = false;
        evt.Update(5.0f);
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);
        Assert.That(didRun, Is.True);
    }

    [Test]
    public void LargeDeltaTimeEventRunTest()
    {
        int tally = 0;

        ScheduledEvent evt = new ScheduledEvent(
            "test",
            (ev) => { tally++; Debug.ULogChannel("ScheduledEventTest", "Event {0} fired", ev.Name); },
            3.0f,
            true,
            0);

        // event should fire three times in 10 seconds
        evt.Update(10f);
        Assert.That(tally, Is.EqualTo(3));

        // timer should be 4 * 3 - 10 = 2 seconds
        Assert.That(evt.TimeToWait, Is.EqualTo(2));

        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);
    }

    [Test]
    public void StopEventRunTest()
    {
        ScheduledEvent evt = new ScheduledEvent(
            "test",
            callback,
            3.0f,
            true,
            1);

        // doesn't run until the cooldown is reached
        didRun = false;
        evt.Update(5.0f);
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.False);
        Assert.That(didRun, Is.True);

        evt.Stop();

        // and now it should not run
        Assert.That(evt.LastShot, Is.False);
        Assert.That(evt.Finished, Is.True);
        didRun = false;
        evt.Update(5.0f);
        Assert.That(didRun, Is.False);
    }

    [Test]
    public void WriteXMLTest()
    {
        ScheduledEvent evt = new ScheduledEvent(
            "test",
            (ev) => Debug.ULogChannel("ScheduledEventTest", "Event {0} fired", ev.Name),
            3.0f,
            true,
            1);

        StringBuilder sb = new StringBuilder();
        XmlWriter writer = new XmlTextWriter(new StringWriter(sb));
        evt.WriteXml(writer);

        Assert.That(sb.ToString(), Is.EqualTo("<Event name=\"test\" cooldown=\"3\" timeToWait=\"3\" repeatsForever=\"True\" />"));

        evt = new ScheduledEvent(
            "test",
            callback,
            3.0f,
            false,
            2);

        sb = new StringBuilder();
        writer = new XmlTextWriter(new StringWriter(sb));
        evt.WriteXml(writer);

        Assert.That(sb.ToString(), Is.EqualTo("<Event name=\"test\" cooldown=\"3\" timeToWait=\"3\" repeatsLeft=\"2\" />"));
    }
}
