using System;
using System.Collections.Generic;
using System.Threading;

public enum SecurityLevel
{
    Confidential,
    Secret,
    TopSecret
}

public class Elevator
{
    public int CurrentFloor { get; private set; }
    public bool IsMoving { get; private set; }
    private Queue<int> RequestedFloors { get; set; } = new Queue<int>();
    private object lockObject = new object();

    public void MoveToFloor(int destinationFloor)
    {
        IsMoving = true;
        Thread.Sleep(1000 * Math.Abs(destinationFloor - CurrentFloor));
        CurrentFloor = destinationFloor;
        IsMoving = false;
    }

    public void OpenDoor(Agent agent)
    {
        Console.WriteLine($"Elevator door opens on floor {CurrentFloor}");
        if (ElevatorDoor.SecurityCheck(agent))
        {
            Console.WriteLine($"Agent with {agent.SecurityLevel} level enters the elevator.");
        }
        else
        {
            Console.WriteLine("Security check failed. Agent cannot enter.");
        }
    }

    public void CloseDoor()
    {
        Console.WriteLine("Elevator door closes.");
    }

    public void PressButton(int floor)
    {
        lock (lockObject)
        {
            RequestedFloors.Enqueue(floor);
        }
    }

    public void ElevatorThread()
    {
        while (true)
        {
            if (RequestedFloors.Count > 0)
            {
                int destinationFloor;
                lock (lockObject)
                {
                    destinationFloor = RequestedFloors.Dequeue();
                }

                MoveToFloor(destinationFloor);
                OpenDoor(new Agent(SecurityLevel.Confidential, 0, this));
                CloseDoor();
            }
            else
            {
                Thread.Sleep(1000);
            }
        }
    }
}
public class ElevatorDoor
{
    public static bool SecurityCheck(Agent agent)
    {
        switch (agent.SecurityLevel)
        {
            case SecurityLevel.Confidential:
                return agent.CurrentFloor == 0;
            case SecurityLevel.Secret:
                return agent.CurrentFloor <= 1;
            case SecurityLevel.TopSecret:
                return agent.CurrentFloor <= 3; 
            default:
                return false;
        }
    }
}
public class Agent
{
    public SecurityLevel SecurityLevel { get; private set; }
    public int CurrentFloor { get; private set; }
    public int DestinationFloor { get; private set; }
    private Elevator elevator;

    public Agent(SecurityLevel securityLevel, int currentFloor, Elevator elevator)
    {
        SecurityLevel = securityLevel;
        CurrentFloor = currentFloor;
        this.elevator = elevator;
    }

    public void PressElevatorButton()
    {
        elevator.PressButton(CurrentFloor);
    }

    public void EnterElevator()
    {
        elevator.OpenDoor(this);
        DestinationFloor = new Random().Next(4);
        elevator.PressButton(DestinationFloor);
        elevator.CloseDoor();
    }

    public void ExitElevator()
    {
        elevator.OpenDoor(this);
        elevator.CloseDoor();
    }

    public void AgentThread()
    {
        while (true)
        {
            PressElevatorButton();
            EnterElevator();
            ExitElevator();
            CurrentFloor = DestinationFloor;
            Thread.Sleep(1000);
        }
    }
}

class Program
{
    static void Main()
    {
        Elevator elevator = new Elevator();
        Thread elevatorThread = new Thread(elevator.ElevatorThread);
        elevatorThread.Start();

        Agent confidentialAgent = new Agent(SecurityLevel.Confidential, 0, elevator);
        Agent secretAgent = new Agent(SecurityLevel.Secret, 0, elevator);
        Agent topSecretAgent = new Agent(SecurityLevel.TopSecret, 0, elevator);

        Thread confidentialThread = new Thread(confidentialAgent.AgentThread);
        Thread secretThread = new Thread(secretAgent.AgentThread);
        Thread topSecretThread = new Thread(topSecretAgent.AgentThread);

        confidentialThread.Start();
        secretThread.Start();
        topSecretThread.Start();
    }
}
