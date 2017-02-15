# UnityComponentRegistry #
## Examples ##

###Inherit from IComponentRegistry object###

`public class Unit : IComponentRegistry
{
}`

###or register/unregister your components directly using###

`this.RegisterComponent();`

`this.UnregisterComponent();`

###To get access to given coponent types###

`List<Component> components = this.GetObjectsOfType<Unit>()`

###For sorting use following component list extension methods:###

Raycast: 
`this.GetObjectsOfType<Unit>().Raycast(go, layer, 30f);`

RaycastInverse: 
`this.GetObjectsOfType<Unit>().RaycastInverse(go, layer, 30f);`

SortByDistance: 
`this.GetObjectsOfType<Unit>().SortByDistance(new Vector3(0f, 0f, 0f));`

SortByDistanceInverse: 
`this.GetObjectsOfType<Unit>().SortByDistanceInverse(new Vector3(0f, 0f, 0f));`

First: 
`this.GetObjectsOfType<Unit>().First(2);`

Last: 
`this.GetObjectsOfType<Unit>().Last(2);`

Random: 
`this.GetObjectsOfType<Unit>().Random(2);`

Range: 
`this.GetObjectsOfType<Unit>().Range(new Vector3(0f, 0f, 0f), 5f 10f);`

Inverse: 
`this.GetObjectsOfType<Unit>().Inverse(typeof(Unit));`

###It's also possible to link them together###
`List<Component> components = this.GetObjectsOfType<Unit>().SortByDistance(new Vector3(0f, 0f, 0f)).RaycastInverse(go, layer, 30f).Random(2);`

Note!
This code is very immature and experimental.
