﻿using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static WaterSystem.BuoyantObject_DOTS;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Physics;

[UpdateAfter(typeof(InputSystem))]
public class UpdateObjectTransformSystem : JobComponentSystem
{
	NativeArray<Entity> entities;
	NativeArray<float3> positions;
	NativeArray<quaternion> rotations;


	protected override void OnCreate()
	{
		entities = new NativeArray<Entity>(512, Allocator.Persistent);
		positions = new NativeArray<float3>(512, Allocator.Persistent);
		rotations = new NativeArray<quaternion>(512, Allocator.Persistent);

		base.OnCreate();
	}

	protected override void OnDestroy()
	{
		entities.Dispose();
		positions.Dispose();
		rotations.Dispose();

		base.OnDestroy();
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		for (int i = 0; i < entities.Length; i++)
		{
			var t = DOTSTransformManager.GetTransform(entities[i]);
			if(t != null)
				t.SetPositionAndRotation(positions[i], rotations[i]);
		}

		var job = new SyncJob
		{
			entities = entities,
			positions = positions,
			rotations = rotations
		};

		return job.Schedule(this, inputDeps);
	}

	[BurstCompile]
	[RequireComponentTag(typeof(SyncTransformTag))]
	public struct SyncJob : IJobForEachWithEntity<Translation, Rotation>
	{
		public NativeArray<Entity> entities;
		public NativeArray<float3> positions;
		public NativeArray<quaternion> rotations;


		public void Execute(Entity entity, int index, ref Translation pos, ref Rotation rot)
		{
			entities[index] = entity;
			positions[index] = pos.Value;
			rotations[index] = rot.Value;
		}
	}
}