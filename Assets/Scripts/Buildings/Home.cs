using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum HouseType
{
    small,
    medium,
    large
}

public class Home : MonoBehaviour
{
    public HouseType houseType;

    [HideInInspector] public List<AIController> people;
    [HideInInspector] public int maxPeople;
    
    public void Start()
    {
        switch (houseType)
        {
            case HouseType.small:
                maxPeople = 5;
                break;
            case HouseType.medium:
                maxPeople = 10;
                break;
            case HouseType.large:
                maxPeople = 15;
                break;
            default:
                maxPeople = 5;
                break;
        }

        people = new List<AIController>();
    }

    public void AddPerson(AIController person)
    {
        people.Add(person);
    }

    public void RemovePerson(AIController person)
    {
        people.Remove(person);
    }
}
