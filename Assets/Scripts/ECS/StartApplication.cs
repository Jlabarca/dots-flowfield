using UnityEngine;

public class StartApplication : MonoBehaviour
{
    private bool HasInit;
    private void Update()
    {
        //This is not working
        if (HasInit) return;
        HasInit = true;
        GameInitializer.InitializeSystemWorkflow();
        gameObject.SetActive(false);
    }
}