using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ComponentRegistry
{
    #region STATIC MAPPING
    /// <summary>
    /// Temporary list used for component sorting purposes
    /// </summary>
    static List<Component> bufferList = new List<Component>();

    /// <summary>
    /// Register dictionary to keep track of each component type and their instances.
    /// </summary>
    static Dictionary<System.Type, List<Component>> typeToComponentMap = new Dictionary<System.Type, List<Component>>();

    static List<Component> GetObjectsOfType(System.Type type)
    {
        if (!typeToComponentMap.ContainsKey(type))
            typeToComponentMap[type] = new List<Component>();

        return typeToComponentMap[type];
    }

    /// <summary>
    /// Get components of given generic type. if no list is provided new lint instance is allocated.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    /// <param name="comp">Extension</param>
    /// <param name="duplicates">Enable duplicating items in list</param>
    /// <param name="list">List to store components. Can be null.</param>
    /// <returns>List of found components</returns>
    public static List<Component> GetComponentsOfType<T>(
        this Component comp,
        List<Component> list = null) where T : Component
    {
        if (list == null)
            list = new List<Component>();

        System.Type type = typeof(T);
        if (!typeToComponentMap.ContainsKey(type))
            return list;

        List<Component>.Enumerator iter = typeToComponentMap[type].GetEnumerator();
        while (iter.MoveNext())
        {
            Component o = iter.Current;
            list.Add(o);
        }
        return list;
    }

    /// <summary>
    /// Get components of given generic type. Found components are added to supplied list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="comp">Extension</param>
    /// <param name="duplicates">Enable duplicating items in list</param>
    /// <param name="list">Reference to list</param>
    /// <returns>List of found components</returns>
    public static List<Component> GetComponentsOfTypeNonAlloc<T>(
        this Component comp,
        ref List<Component> list) where T : Component
    {
        System.Type type = typeof(T);
        if (!typeToComponentMap.ContainsKey(type))
            return list;

        List<Component>.Enumerator iter = typeToComponentMap[type].GetEnumerator();
        while (iter.MoveNext())
        {
            Component o = iter.Current;
            list.Add(o);
        }
        return list;
    }

    /// <summary>
    /// Register this component to static map.
    /// </summary>
    /// <param name="comp"></param>
    public static void RegisterComponent(this Component comp)
    {
        if (!typeToComponentMap.ContainsKey(comp.GetType()))
            typeToComponentMap[comp.GetType()] = new List<Component>();

        List<Component> comps = typeToComponentMap[comp.GetType()];

        if (comps.Contains(comp))
            return;

        comps.Add(comp);
    }

    /// <summary>
    /// Remove component from static map.
    /// </summary>
    /// <param name="comp"></param>
    public static void UnregisterComponent(this Component comp)
    {
        if (!typeToComponentMap.ContainsKey(comp.GetType()))
            return;

        List<Component> comps = typeToComponentMap[comp.GetType()];
        comps.Remove(comp);
    }
    #endregion
    #region COMPONENT LIST EXTENSIONS

    /// <summary>
    /// To check if there is no obsticles between the component object and target game object.
    /// Remove any components from the list, that don't have direct vision to target.
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="go">Target gameobject</param>
    /// <param name="layer">Layer mask</param>
    /// <param name="distance">Max ray distance</param>
    /// <returns></returns>
    public static List<Component> Raycast(this List<Component> list, GameObject go, LayerMask layer, float distance = float.MaxValue)
    {
        RaycastHit hit;
        Ray ray = new Ray(go.transform.position, Vector3.forward);
        for (int i = list.Count - 1; i >= 0; --i)
        {
            Component o = list[i];
            if (o)
            {
                ray.origin = o.transform.position;
                ray.direction = (go.transform.position - o.transform.position).normalized;

                if (Physics.Raycast(ray, out hit, distance, layer))
                {
                    if (hit.collider.gameObject != go.gameObject)
                        list.Remove(o);
                }
                else
                    list.Remove(o);
            }
        }
        return list;
    }

    /// <summary>
    /// If object dont have direct vision to given target object, keep it on list.
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="go">Target gameobject</param>
    /// <param name="layer">Layer mask</param>
    /// <param name="distance">Max ray distance</param>
    /// <returns></returns>
    public static List<Component> RaycastInverse(this List<Component> list, GameObject go, LayerMask layer, float distance = float.MaxValue)
    {
        RaycastHit hit;
        Ray ray = new Ray(go.transform.position, Vector3.forward);

        for (int i = list.Count - 1; i >= 0; --i)
        {
            Component o = list[i];
            if (o)
            {
                ray.origin = o.transform.position;
                ray.direction = (go.transform.position - o.transform.position).normalized;

                if (Physics.Raycast(ray, out hit, distance, layer))
                {
                    if (hit.collider.gameObject == go.gameObject)
                        list.Remove(o);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// Check if ray hits any of the components.
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="ray">Target ray</param>
    /// <param name="layer">Layer mask</param>
    /// <param name="distance">Max raycast distance</param>
    /// <returns>Hitted compoennt or null</returns>
    public static Component RayHit(this List<Component> list, Ray ray, LayerMask layer, float distance = float.MaxValue)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance, layer))
        {
            List<Component>.Enumerator iter = list.GetEnumerator();
            while(iter.MoveNext())
            {
                Component c = iter.Current;
                if (c.gameObject == hit.collider.gameObject)
                    return c;
            }
        }
        return null;
    }

    /// <summary>
    /// Sort list of components from neartest to farthest
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Target point</param>
    /// <returns>Sorted list</returns>
    public static List<Component> SortByDistance(this List<Component> list, Vector3 point)
    {
        list.Sort((x, y) => (x.transform.position - point).sqrMagnitude.CompareTo((y.transform.position - point).sqrMagnitude));
        return list;
    }

    /// <summary>
    /// Sort list of components from farthest to neartest
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Target point</param>
    /// <returns>Sorted list</returns>
    public static List<Component> SortByDistanceInverse(this List<Component> list, Vector3 point)
    {
        list.Sort((x, y) => (y.transform.position - point).sqrMagnitude.CompareTo((x.transform.position - point).sqrMagnitude));
        return list;
    }

    /// <summary>
    /// Get list of components between min and max distance to point.
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Target point</param>
    /// <param name="min">Min distance</param>
    /// <param name="max">Max dinstance</param>
    /// <returns></returns>
    public static List<Component> Range(this List<Component> list, Vector3 point, float min = 0f, float max = float.MaxValue)
    {
        Component o = null;
        float distance = 0f;
        float minSqr = min * min;
        float maxSqr = max * max;
        for (int i = list.Count - 1; i >= 0; --i)
        {
            o = list[i];
            if (o == null)
                continue;

            //float distance = Vector3.Distance(point, o.transform.position);
            distance = (point - o.transform.position).sqrMagnitude;
            if (distance >= minSqr && distance <= maxSqr)
                continue;

            list.Remove(o);
            o = null;
        }
        return list;
    }

    /// <summary>
    /// Only keep first component on list and remove others.
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="count">Return component count</param>
    /// <returns></returns>
    public static List<Component> First(this List<Component> list, int count = 1)
    {
        count = Mathf.Clamp(count, 1, list.Count);
        return list.GetRange(0, count);
    }

    /// <summary>
    /// Only keep last coponents on list and remove others.
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="count">*Return component count</param>
    /// <returns></returns>
    public static List<Component> Last(this List<Component> list, int count = 1)
    {
        count = Mathf.Clamp(count, 1, list.Count);
        return list.GetRange(list.Count - count, count);
    }

    /// <summary>
    /// Pick random of components from list.
    /// </summary>
    /// <param name="list">Extensions</param>
    /// <param name="count">Number of random components</param>
    /// <returns></returns>
    public static List<Component> Random(this List<Component> list, int count = 1)
    {
        count = Mathf.Clamp(count, 1, list.Count);
        while (count < list.Count)
        {
            Component o = list[UnityEngine.Random.Range(0, list.Count)];
            list.Remove(o);
        }
        return list;
    }

    /// <summary>
    /// Return inverted list of given component type.
    /// @TODO Optimize this function
    /// </summary>
    /// <param name="list">Extensions</param>
    /// <param name="type">Component type</param>
    /// <returns></returns>
    public static List<Component> Inverse(this List<Component> list, System.Type type)
    {
        bufferList.Clear();

        Component c1 = null;
        List<Component>.Enumerator fullListIter = GetObjectsOfType(type).GetEnumerator();
        while(fullListIter.MoveNext())
        {
            c1 = fullListIter.Current;
            if (!list.Contains(c1))
                bufferList.Add(c1);
        }

        list.Clear();
        for (var i = 0; i < bufferList.Count; ++i)
        {
            Component c = bufferList[i];
            list.Add(c);
        }

        return list;
    }

    #endregion
}
