using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Farm
{
    public Building_Farm farm;
    public bool hasWorker;
    public Farm (Building_Farm farm)
    {
        this.farm = farm;
        hasWorker = false;
    }
}
public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    //List<Home> homes;
    //List<Farm> farms;

    //private void Start()
    //{
    //    homes = new List<Home>();
    //    farms = new List<Farm>();
    //}


    //public void AddHome(Home home)
    //{
    //    homes.Add(home);
    //}

    //public void RemoveHome(GameObject obj)
    //{
    //    homes.Remove(GetHome(obj));
    //}

    //public void AddFarm(Building_Farm building)
    //{
    //    farms.Add(new Farm(building));
    //}

    //public void RemoveFarm(Building_Farm farm)
    //{
    //    farms.Remove(GetFarm(farm));
    //}

    //private Farm GetFarm(Building_Farm farm)
    //{
    //    foreach (Farm f in farms)
    //    {
    //        if (f.farm == farm)
    //        {
    //            return f;
    //        }
    //    }

    //    return null;
    //}

    //private Home GetHome(GameObject home)
    //{
    //    foreach (Home h in homes)
    //    {
    //        if (h.obj == home)
    //        {
    //            return h;
    //        }
    //    }

    //    return null;
    //}

    //public Building_Farm GetFarmBuilding()
    //{
    //    foreach (Farm farm in farms)
    //    {
    //        if (!farm.hasWorker)
    //        {
    //            farm.hasWorker = true;
    //            return farm.farm;
    //        }
    //    }

    //    return null;
    //}

    //public void ResetFarms()
    //{
    //    foreach (Farm farm in farms)
    //    {
    //        farm.hasWorker = false;
    //    }
    //}

    //public Home AssignRandomOpenHome(AIController person)
    //{
    //    foreach (Home home in homes)
    //    {
    //        if (home.people.Count < home.maxPeople)
    //        {
    //            return home;
    //        }
    //    }

    //    //no open homes
    //    return null;
    //}


}
