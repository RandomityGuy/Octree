# Octree
An easy to use implementation of octree for 3D points with insertion, deletion and KNN

## Creation
```cs
var points = new List<Vector3>();
// The code to add your points
var octree = new Octree(points);
```

## Insertion
```cs
var point = new Vector3(1, 2, 3);
octree.Insert(point);
```

## Deletion
```cs
var point = new Vector3(1, 2, 3);
octree.Remove(point);
```

## K Nearest Neighbours
```cs
var point = new Vector3(1, 2, 3);
var points = octree.KNN(point, 5); // Searches for 5 closest points to given point
```

## Radius Search
```cs
var point = new Vector3(1, 2, 3);
var points = octree.RadiusSearch(point, 5, 200); // Searches for maximum 5 points closest to given points in radius of 200 units
```
