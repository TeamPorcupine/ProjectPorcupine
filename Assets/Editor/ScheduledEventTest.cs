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
using UnityEditor;
using UnityEngine;

public class ScheduledEventTest
{
    private Action<ScheduledEvent> callback;
    private bool didRun = false;

    [SetUp]
    public void Init()
    {
        callback = (e) =>
            {
                Debug.ULogChannel("ScheduledEventTest", "Event {0} fired", e.Name);
                didRun = true;
            };
    }

    [Test]
    public void EventCreationTest()
    {
        ScheduledEvent evt = new ScheduledEvent(
            "test",
            (e) => Debug.ULogChannel("ScheduledEventTest", "Event {0} fired", e.Name),
            3.0f,
            true,
            1);

        ScheduledEvent evt2 = new ScheduledEvent(evt);

        Assert.That(evt.Name, Is.EqualTo("test"));
        Assert.That(evt2.Name, Is.EqualTo("test"));
        Assert.That(evt, Is.Not.EqualTo(evt2));
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
            "test3",
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
            "test4",
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
    public void StopEventRunTest()
    {
        ScheduledEvent evt = new ScheduledEvent(
            "test5",
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
}
