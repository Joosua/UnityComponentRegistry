using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // @TODO remove!!!

public static class ComponentRegistry
{
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
    /// Get components of given generic type. If list reference is not passed, new component list is instantiated.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    /// <param name="comp">Target component</param>
    /// <param name="list">Reference list to store components. Can be null.</param>
    /// <returns></returns>
    public static List<Component> GetObjectsOfType<T>(this Component comp, List<Component> list = null) where T : Component
    {
        if (list == null)
            list = new List<Component>();

        System.Type type = typeof(T);
        if (!typeToComponentMap.ContainsKey(type))
            return null;

        for (var i = 0; i < typeToComponentMap[type].Count; ++i)
        {
            Component o = typeToComponentMap[type][i];
            if (!list.Contains(o))
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

    #region COMPONENT LIST EXTENSIONS

    /// <summary>
    /// To check if there is no obsticles between the component object and target game object.
    /// Remove any component that dont have direct vision to target game object or the distance is too great.
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
    /// If object dont have direct vision to given game object, keep it on list.
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
    /// Sort list of components from neartest to farthest
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Target point</param>
    /// <returns>Sorted list</returns>
    public static List<Component> SortByDistance(this List<Component> list, Vector3 point)
    {
        return list.OrderBy(o => Vector3.Distance(point, o.transform.position)).ToList();
    }

    /// <summary>
    /// Sort list of components from farthest to neartest
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Target point</param>
    /// <returns>Sorted list</returns>
    public static List<Component> SortByDistanceInverse(this List<Component> list, Vector3 point)
    {
        return list.OrderByDescending(o => Vector3.Distance(point, o.transform.position)).ToList();
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
    /// Get list of components between min and max distance to point.
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Target point</param>
    /// <param name="min">Min distance</param>
    /// <param name="max">Max dinstance</param>
    /// <returns></returns>
    public static List<Component> Range(this List<Component> list, Vector3 point, float min = 0f, float max = float.MaxValue)
    {
        for (int i = list.Count - 1; i >= 0; --i)
        {
            Component o = list[i];
            if (o == null)
                continue;

            float distance = Vector3.Distance(point, o.transform.position);
            if (distance >= min && distance <= max)
                continue;

            list.Remove(o);
        }
        return list;
    }


    /// <summary>
    /// Compare each component on list to restry and return inversed list of components
    /// @TODO Optimize this function
    /// @TODO Remove type parameter
    /// </summary>
    /// <param name="list">Extensions</param>
    /// <param name="type">Component type</param>
    /// <returns></returns>
    public static List<Component> Inverse(this List<Component> list, System.Type type)
    {
        List<Component> fullList = GetObjectsOfType(type);
        bufferList.Clear();

        Component c1 = null;
        for (var i = 0; i < fullList.Count; ++i)
        {
            c1 = fullList[i];
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
