using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIResearcherController : AIController
{
    AIMoveTo aiMoveTo;

    [SerializeField] GameObject selectedObj;
    [SerializeField] SpriteRenderer hintSprite;
    [SerializeField] GameObject arrowObj;

    Vector3 targetLoc = Vector3.negativeInfinity;
    float donePathingDistance;
    float arrivedDistance;
    float orbArrivedDistance;
    bool donePathing = false;

    List<GameObject> foundOrbs;
    GameObject trackingOrb = null;

    bool hasStarted = false;

    public bool hasSearch = false;
    public bool isAuto = false;

    // Start is called before the first frame update
    void Start()
    {
        aiMovement = this.GetComponent<AIMovement>();
        aiManager = GameObject.Find(Constants.GAMEOBJECT_AIMANAGER).GetComponent<AIManager>();
        aiMoveTo = new AIMoveTo();
        info.job = new Job(Vector3.zero, JobType.work_building, null, 0);

        foundOrbs = new List<GameObject>();

        donePathingDistance = 1;
        arrivedDistance = 0.2f;
        orbArrivedDistance = 0.2f;
        
        hasStarted = true;
    }

    public void SetHasSearch(bool activeSearch)
    {
        hasSearch = activeSearch;
        if (hasSearch)
        {
            arrowObj.SetActive(true);
        }
    }

    public void SetHasAuto(bool hasAuto)
    {
        isAuto = hasAuto;
    }

    // Update is called once per frame
    public override void UpdateController()
    {
        if (!hasStarted) return;

        SetHintColor();

        if (RevealResearchOrbs())
        {
            if (trackingOrb == null)
            {
                trackingOrb = GetClosestOrb();
                if (trackingOrb == null)
                {
                    return;
                }
            }


            if (trackingOrb != null)
            {
                aiMovement.MoveToPosition(trackingOrb.transform.position);
            }


            if ((aiMovement.Position - trackingOrb.transform.position).magnitude <= arrivedDistance)
            {
                foundOrbs.Remove(trackingOrb);
                OrbManager.Instance.RemoveOrb(trackingOrb);
                int ranAmount = Random.Range(2, 5);
                ResourceManager.Instance.AddResource(ResourceType.researchOrb, ranAmount);
                UIManager.Instance.AddInfoMessage($"You collected {ranAmount} researchOrbs");
                trackingOrb = null;

                if (foundOrbs.Count == 0)
                {
                    //redo pathfinding
                    aiMoveTo.StartJob(this, aiMovement);
                }
            }


        }
        else if (!targetLoc.Equals(Vector3.negativeInfinity))
        {
            float distance = (targetLoc - aiMovement.Position).magnitude;
            //pathing to target
            if (distance > arrivedDistance)
            {
                if (distance > donePathingDistance)
                {
                    aiMoveTo.WorkJob(this, aiMovement);
                }
                else if (!donePathing)
                {
                    donePathing = true;
                }
            }
            //at target
            else
            {
                targetLoc = Vector3.negativeInfinity;
            }
        }
        //deal with auto
        else if (targetLoc.Equals(Vector3.negativeInfinity) && isAuto)
        {
            Vector3 orbPos = OrbManager.Instance.GetClosestOrbFromPosition(this.transform.position);

            if (foundOrbs == null) foundOrbs = new List<GameObject>();
            foundOrbs.AddRange(OrbManager.Instance.RevealOrbFromPosition(orbPos, 1f));
        }


        aiMovement.UpdateMovement();



        //deal with search
        if (hasSearch)
        {
            Vector3 position = OrbManager.Instance.GetClosestOrbFromPosition(this.transform.position);

            Vector3 forward = (this.transform.position - position).normalized;
            arrowObj.transform.forward = forward;
        }

    }

    private void SetHintColor()
    {
        float hotDistance = 5f;
        float coldDistance = 15f;

        Vector3 orbPos = OrbManager.Instance.GetClosestOrbFromPosition(this.transform.position);
        float distance = (this.transform.position - orbPos).magnitude;

        if (distance >= coldDistance)
        {
            hintSprite.color = Color.red;
        }
        else if (distance <= hotDistance)
        {
            hintSprite.color = Color.green;
        }
        else
        {
            hintSprite.color = Color.Lerp(Color.green, Color.red, distance / 10f);
        }

    }

    private bool RevealResearchOrbs()
    {
        if (foundOrbs == null) foundOrbs = new List<GameObject>();

        foundOrbs.AddRange(OrbManager.Instance.RevealOrbFromPosition(this.transform.position, 5f));

        return foundOrbs.Count > 0;
    }

    private GameObject GetClosestOrb()
    {
        GameObject lowestDistOrb = null;
        float lowestDist = -1;
        for (int i = 0; i < foundOrbs.Count; i++)
        { 
            if (foundOrbs[i] == null)
            {
                foundOrbs.RemoveAt(i);
                break;
            }

            float dist = (this.transform.position - foundOrbs[i].transform.position).magnitude;
            if (dist < lowestDist || lowestDist == -1)
            {
                lowestDistOrb = foundOrbs[i];
                lowestDist = dist;
            }
        }

        return lowestDistOrb;
    }

    public void MoveToClickPos(Vector3 clickPos)
    {
        targetLoc = clickPos;
        info.job.jobPos = clickPos;
        aiMoveTo.StartJob(this, aiMovement);
    }

    public void SetSelectUnit(bool active)
    {
        if (selectedObj == null)
            return;

        selectedObj.SetActive(active);
    }

}
