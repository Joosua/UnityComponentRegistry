# UnityComponentRegistry #
##How to use##

###Inherit from IComponentRegistry object###

`public class Unit : IComponentRegistry
{
}`

###or register/unregister your components directly using###

`this.RegisterComponent();`

`this.UnregisterComponent();`

###To get access to given coponent types###

`List<Component> components = this.GetComponentsOfType<Unit>()`

###And for non alloc you can use###

`this.GetComponentsOfTypeNonAlloc<Unit>(ref list)`

###For sorting use following component list extension methods:###

**SortByDistance:**
`this.GetObjectsOfType<Unit>().SortByDistance(point);`

**SortByDistanceInverse:**
`this.GetObjectsOfType<Unit>().SortByDistanceInverse(point);`

**First:**
`this.GetObjectsOfType<Unit>().First(2);`

**Last:**
`this.GetObjectsOfType<Unit>().Last(2);`

**Random:**
`this.GetObjectsOfType<Unit>().Random(2);`

**Range:**
`this.GetObjectsOfType<Unit>().Range(point, 5f 10f);`

**Raycast:**
`this.GetObjectsOfType<Unit>().Raycast(go, layer, 30f);`

**RaycastInverse:**
`this.GetObjectsOfType<Unit>().RaycastInverse(go, layer, 30f);`

**RayHit:**
`Component c = this.GetObjectsOfType<Unit>().RayHit(ray, layer, 30f);`

**OverlapSphere:**
`this.GetObjectsOfType<Unit>().OverlapSphere(point, radius, layer);`

**OverlapBox:**
`this.GetObjectsOfType<Unit>().OverlapBox(point, halfExtends, orentation, layer);`

**OverlapCapsule:**
`this.GetObjectsOfType<Unit>().OverlapCapsule(point, radius, height, direction, orientation, layer);`

**OverlapCollider:**
`this.GetObjectsOfType<Unit>().OverlapCollider(collider, layer);`

**Inverse:**
`this.GetObjectsOfType<Unit>().Inverse(typeof(Unit));`

###It's also possible to link them together###
`List<Component> components = this.GetObjectsOfType<Unit>().SortByDistance(new Vector3(0f, 0f, 0f)).RaycastInverse(go, layer, 30f).Random(2);`

Note!
This code is very immature and experimental.
