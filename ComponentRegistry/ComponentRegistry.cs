using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ComponentRegistry
{
    #region STATIC MAPPING

    /// <summary>
    /// Buffer used by overlapping test
    /// </summary>
    static Collider[] colliderBuffer = new Collider[100];

    /// <summary>
    /// Temporary list used for component sorting purposes
    /// </summary>
    static List<Component> bufferList = new List<Component>();

    /// <summary>
    /// Register dictionary to keep track of each component type and their instances.
    /// </summary>
    static Dictionary<System.Type, List<Component>> typeToComponentMap = new Dictionary<System.Type, List<Component>>();

    static List<Component> ComponentsFromBuffer(this List<Component> list, ref Collider[] buffer, int count)
    {
        bufferList.Clear();
        List<Component>.Enumerator listIter = list.GetEnumerator();
        while (listIter.MoveNext())
        {
            Component c = listIter.Current;

            for (int i = 0; i < count; ++i)
            {
                Collider col = buffer[i];

                if (col.gameObject == c.gameObject)
                {
                    bufferList.Add(c);
                    break;
                }
            }
        }

        list.Clear();
        for (var i = 0; i < bufferList.Count; ++i)
        {
            Component c = bufferList[i];
            list.Add(c);
        }

        return list;
    }

    static List<Component> GetObjectsOfType(System.Type type)
    {
        if (!typeToComponentMap.ContainsKey(type))
            typeToComponentMap[type] = new List<Component>();

        return typeToComponentMap[type];
    }

    /// <summary>
    /// Resize collider buffer
    /// </summary>
    /// <param name="newSize">new buffer capacity</param>
    public static void ResizeColliderBuffer(int newSize)
    {
        colliderBuffer = new Collider[newSize];
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
    /// <param name="layers">Layer mask</param>
    /// <param name="distance">Max ray distance</param>
    /// <returns></returns>
    public static List<Component> RaycastInverse(this List<Component> list, GameObject go, LayerMask layers, float distance = float.MaxValue)
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

                if (Physics.Raycast(ray, out hit, distance, layers))
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
    /// <param name="layers">Layer mask</param>
    /// <param name="distance">Max raycast distance</param>
    /// <returns>Hitted compoennt or null</returns>
    public static Component RayHit(this List<Component> list, Ray ray, LayerMask layers, float distance = float.MaxValue)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance, layers))
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

    /// <summary>
    /// Return list of components inside the Box
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Center point</param>
    /// <param name="halfExtends">Box dimentions</param>
    /// <param name="orientation">Box orientation</param>
    /// <param name="layers">Layers</param>
    /// <returns>List of components inside the sphere</returns>
    public static List<Component> OverlapBox(this List<Component> list, Vector3 point, Vector3 halfExtends, Quaternion orientation, LayerMask layers)
    {
        int colliderCount = 0;
        colliderCount = Physics.OverlapBoxNonAlloc(point, halfExtends, colliderBuffer, orientation, layers);

        return ComponentsFromBuffer(list, ref colliderBuffer, colliderCount);
    }

    /// <summary>
    /// Return list of components inside the sphere
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Center point</param>
    /// <param name="radius">radius</param>
    /// <param name="layers">Layers</param>
    /// <returns>List of components inside the sphere</returns>
    public static List<Component> OverlapSphere(this List<Component> list, Vector3 point, float radius, LayerMask layers)
    {
        int colliderCount = 0;
        colliderCount = Physics.OverlapSphereNonAlloc(point, radius, colliderBuffer, layers);

        return ComponentsFromBuffer(list, ref colliderBuffer, colliderCount);
    }

    /// <summary>
    /// Return list of components inside the view cone
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Point</param>
    /// <param name="dir">Cone normalized direction</param>
    /// <param name="distance">Max distance</param>
    /// <param name="angle">Max angle</param>
    /// <returns>List of components inside the cone</returns>
    public static List<Component> ViewCone(this List<Component> list, Vector3 point, Vector3 dir, float distance, float angle)
    {
        Component c = null;
        Vector3 diff;
        float sqrMag = distance * distance;

        for (int i = list.Count - 1; i >= 0; --i)
        {
            c = list[i];
            diff = c.transform.position - point;

            if (diff.sqrMagnitude > sqrMag ||
                Vector3.Angle(diff, dir) > angle)
            {
                list.Remove(c);
            }
        }

        return list;
    }

    /// <summary>
    /// Return list of components inside the capsule
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="point">Center point</param>
    /// <param name="radius">Radius</param>
    /// <param name="height">Height</param>
    /// <param name="direction">Direction where 0 = x-axis, 1 = y axis and 2 = z axis</param>
    /// <param name="orientation">Capsule orientation</param>
    /// <param name="layers">Layers</param>
    /// <returns>List of components inside the capsule</returns>
    public static List<Component> OverlapCapsule(this List<Component> list, Vector3 point, float radius, float height, int direction, Quaternion orientation, LayerMask layers)
    {
        int colliderCount = 0;
        Vector3 offset = Vector3.zero;

        if (direction == 0) // X-axis
            offset = (orientation * Vector3.right) * height * 0.5f;
        else if (direction == 1) // Y-axis
            offset = (orientation * Vector3.up) * height * 0.5f;
        if (direction == 2) // Z-axis
            offset = (orientation * Vector3.forward) * height * 0.5f;
        Vector3 off = offset.normalized * radius;

        if (offset.sqrMagnitude > off.sqrMagnitude)
            colliderCount = Physics.OverlapCapsuleNonAlloc(point + offset - off, point - offset + off, radius, colliderBuffer, layers);
        else
            colliderCount = Physics.OverlapSphereNonAlloc(point, radius, colliderBuffer, layers);

        return ComponentsFromBuffer(list, ref colliderBuffer, colliderCount);
    }

    /// <summary>
    /// Return list of components inside the collider object. Currently BoxCollider, SphereCollider and CapsuleColliders are supported.
    /// Colliders transform effects on overlap check.
    /// </summary>
    /// <param name="list">Extension</param>
    /// <param name="buffer">Buffer of colliders to use in overlap check</param>
    /// <param name="collider">Target collider</param>
    /// <param name="layers">layers</param>
    /// <returns>List of components inside the collider</returns>
    public static List<Component> OverlapCollider(this List<Component> list, Collider collider, LayerMask layers)
    {
        int colliderCount = 0;
        if (collider is BoxCollider)
        {
            BoxCollider box = collider as BoxCollider;
            Vector3 bounds = box.bounds.extents;
            bounds.Scale(box.transform.localScale);
            colliderCount = Physics.OverlapBoxNonAlloc(collider.transform.TransformPoint(box.center), box.bounds.extents, colliderBuffer, collider.transform.rotation, layers);
        }
        else if (collider is SphereCollider)
        {
            SphereCollider sphere = collider as SphereCollider;
            Vector3 localScale = sphere.transform.localScale;
            float maxScale = Mathf.Max(localScale.x, Mathf.Max(localScale.y, localScale.z));
            colliderCount = Physics.OverlapSphereNonAlloc(collider.transform.TransformPoint(sphere.center), sphere.radius * maxScale, colliderBuffer, layers);
        }
        else if (collider is CapsuleCollider)
        {
            CapsuleCollider capsule = collider as CapsuleCollider;
            Vector3 c = collider.transform.TransformPoint(capsule.center);
            Vector3 offset = Vector3.zero;
            float maxScale = 1f;

            if (capsule.direction == 0) // X-axis
            {
                offset = (capsule.transform.localRotation * Vector3.right) * (capsule.height * capsule.transform.localScale.x * 0.5f);
                maxScale = Mathf.Max(capsule.transform.localScale.y, capsule.transform.localScale.z);
            }
            else if (capsule.direction == 1) // Y-axis
            {
                offset = (capsule.transform.localRotation * Vector3.up) * (capsule.height * capsule.transform.localScale.y * 0.5f);
                maxScale = Mathf.Max(capsule.transform.localScale.x, capsule.transform.localScale.z);
            }
            if (capsule.direction == 2) // Z-axis
            {
                offset = (capsule.transform.localRotation * Vector3.forward) * (capsule.height * capsule.transform.localScale.z * 0.5f);
                maxScale = Mathf.Max(capsule.transform.localScale.x, capsule.transform.localScale.y);
            }

            float totalRadius = capsule.radius * maxScale;
            Vector3 off = offset.normalized * totalRadius;

            if (offset.sqrMagnitude > off.sqrMagnitude)
                colliderCount = Physics.OverlapCapsuleNonAlloc(c + offset - off, c - offset + off, totalRadius, colliderBuffer, layers);
            else
                colliderCount = Physics.OverlapSphereNonAlloc(c, totalRadius, colliderBuffer, layers);
        }
        else
        {
            Debug.LogError("Collider type not supported.");
            list.Clear();
            return list;
        }

        return ComponentsFromBuffer(list, ref colliderBuffer, colliderCount);
    }

    /// <summary>
    /// Iterate each component of given type and invoke a function callback for each list component.
    /// </summary>
    /// <typeparam name="T">Casted component type</typeparam>
    /// <param name="list">Extension</param>
    /// <param name="callback">Callback fucntion</param>
    /// <returns></returns>
    public static List<Component> Each<T>(this List<Component> list, System.Func<T, bool> callback) where T : Component
    {
        T comp = null;
        List<Component>.Enumerator listIter = list.GetEnumerator();
        while (listIter.MoveNext())
        {
            comp = listIter.Current as T;
            if (comp && !callback.Invoke(comp))
                break;
        }
        return list;
    }

    /// <summary>
    /// Invoke a function callback for first coponent of given type
    /// </summary>
    /// <typeparam name="T">Casted component type</typeparam>
    /// <param name="list">Extension</param>
    /// <param name="callback">Callback fucntion</param>
    /// <param name="count">Return component count</param>
    /// <returns></returns>
    public static List<Component> First<T>(this List<Component> list, System.Action<T> callback, int count = 1) where T : Component
    {
        int index = 0;
        T comp = null;
        List<Component>.Enumerator listIter = list.GetEnumerator();
        while (listIter.MoveNext())
        {
            comp = listIter.Current as T;
            if (!comp)
                continue;
            callback.Invoke(comp);

            index++;
            if (index >= count)
                break;
        }
        return list;
    }

    /// <summary>
    /// Invoke a function callback for last coponent of given type
    /// </summary>
    /// <typeparam name="T">Casted component type</typeparam>
    /// <param name="list">Extension</param>
    /// <param name="callback">Callback fucntion</param>
    /// <param name="count">Return component count</param>
    /// <returns></returns>
    public static List<Component> Last<T>(this List<Component> list, System.Action<T> callback, int count = 1) where T : Component
    {
        int index = 0;

        bufferList.Clear();
        bufferList.AddRange(list);
        bufferList.Reverse();

        T comp = null;
        List<Component>.Enumerator listIter = bufferList.GetEnumerator();
        while (listIter.MoveNext())
        {
            comp = listIter.Current as T;
            if (!comp)
                continue;
            callback.Invoke(comp);

            index++;
            if (index >= count)
                break;
        }
        return list;
    }

    #endregion
}
