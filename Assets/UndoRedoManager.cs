using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    public Manipulation manipulation;
    int MAX_STEPS = 5;
    private LinkedList<string> actionList = new LinkedList<string>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LogEntry(string input)
    {
        if (actionList.Count >= MAX_STEPS)
        {
            actionList.RemoveFirst(); 
        }
        actionList.AddLast(input);
    }

    public void Undo(string input)
    {
        manipulation.parseFunctions(actionList.Last.Value);

    }

    public void Redo(string input)
    {

    }

    T GetElementAt<T>(LinkedList<T> list, int index)
    {
        if (index < 0 || index >= list.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var current = list.First;
        for (int i = 0; i < index; i++)
        {
            current = current.Next;
        }
        return current.Value;
    }
}
