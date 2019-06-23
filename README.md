# SpriteSheetRenderer
A powerful Unity ECS system to render massive numbers of animated sprites:
##### 500k entities rendered ~60 fps on a high end windows PC inside the editor. Performance should be massively better in a build.
![N|Solid](https://forum.unity.com/attachments/w6i9mdhdjj-gif.440414/)
### How to use:

* 1- Create the Archetype:

```sh
EntityManager entityManager = World.Active.EntityManager;
spriteSheetArchetype = entityManager.CreateArchetype(
  typeof(Position2D),
  typeof(Rotation2D),
  typeof(Scale),
  typeof(SpriteSheet),
  typeof(SpriteSheetAnimation),
  typeof(SpriteSheetMaterial),
  typeof(UvCell)
);
```

* 2- Spawn a HUUUUGE number of entities:

```sh
NativeArray<Entity> entities = new NativeArray<Entity>(500000, Allocator.Temp);
entityManager.CreateEntity(spriteSheetArchetype, entities);
```

* 3- Fill the values of each entity:

```sh
KeyValuePair<Material, float4[]> atlasData = SpriteSheetCache.BakeSprites(sprites);
spritesCount = atlasData.Value.Length;
for(int i = 0; i < entities.Length; i++) {
  float2 position = A_RANDOM_POSITION;
  entityManager.SetComponentData(entities[i], new Position2D { Value = position });
  entityManager.SetComponentData(entities[i], new Scale { Value = UnityEngine.Random.Range(0.1f, 1f) });
  entityManager.SetComponentData(entities[i], new SpriteSheet { spriteIndex = UnityEngine.Random.Range(0, spritesCount), maxSprites = spritesCount });
  entityManager.SetComponentData(entities[i], new SpriteSheetAnimation { play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 });
  entityManager.SetSharedComponentData(entities[i], new SpriteSheetMaterial { material = atlasData.Key });
  //store the uvs into a dynamic buffer
  var lookup = entityManager.GetBuffer<UvBuffer>(entities[i]);
  for(int j = 0; j < atlasData.Value.Length; j++)
  	buffer.Add(atlasData.Value[j]);
}
entities.Dispose();
```
