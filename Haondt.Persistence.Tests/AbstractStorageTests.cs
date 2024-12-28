using FluentAssertions;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Exceptions;
using Haondt.Persistence.Services;

namespace Haondt.Persistence.Tests
{
    public class Car
    {
        public virtual required string Color { get; set; }
    }

    public class ErrorCar : Car
    {
        public override required string Color { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
    }

    public class Manufacturer
    {
        public required string Name { get; set; }
    }

    public class Tire
    {
        public required int Diameter { get; set; }
    }

    public class NullContainer
    {
        public Car? Value { get; set; }
    }

    public abstract class AbstractStorageTests(IStorage storage)
    {
        [Fact]
        public async Task WillSetAndGetStorageKey()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            await storage.Set(key, new Car { Color = "red" });
            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("red");
        }

        [Fact]
        public async Task WillAddStorageKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var id4 = Guid.NewGuid().ToString();
            var foreignId = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            var key4 = StorageKey<Car>.Create(id4);
            var foreignKey = StorageKey<Car>.Create(foreignId);
            await storage.Add(key, new Car { Color = "red" });
            await storage.Add(key2, new Car { Color = "blue" }, [foreignKey]);
            await storage.AddMany([((StorageKey)key3, new Car { Color = "green" })]);
            await storage.AddMany<Car>([(key4, new Car { Color = "yellow" })]);

            var car = await storage.Get(key);
            car.IsSuccessful.Should().BeTrue();
            car.Value.Color.Should().Be("red");

            var car2 = await storage.Get(key2);
            car2.IsSuccessful.Should().BeTrue();
            car2.Value.Color.Should().Be("blue");

            var car3 = await storage.Get(key3);
            car3.IsSuccessful.Should().BeTrue();
            car3.Value.Color.Should().Be("green");

            var car4 = await storage.Get(key4);
            car4.IsSuccessful.Should().BeTrue();
            car4.Value.Color.Should().Be("yellow");
        }

        [Fact]
        public async Task WillThrowErrorWhenAddingExistingStorageKey()
        {
            var id = Guid.NewGuid().ToString();
            var foreignId = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var foreignKey = StorageKey<Car>.Create(foreignId);
            await storage.Set(key, new Car { Color = "red" });

            await storage.Invoking(s => s.Add(key, new Car { Color = "blue" }))
                .Should().ThrowAsync<StorageKeyExistsException>();
            await storage.Invoking(s => s.Add(key, new Car { Color = "blue" }, [foreignKey]))
                .Should().ThrowAsync<StorageKeyExistsException>();
            await storage.Invoking(s => s.AddMany([((StorageKey)key, new Car { Color = "blue" })]))
                .Should().ThrowAsync<StorageKeyExistsException>();
            await storage.Invoking(s => s.AddMany<Car>([(key, new Car { Color = "blue" })]))
                .Should().ThrowAsync<StorageKeyExistsException>();

            var car = await storage.Get(key);
            car.IsSuccessful.Should().BeTrue();
            car.Value.Color.Should().Be("red");
        }

        [Fact]
        public async Task WillSetAndGetNullContents()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<NullContainer>.Create(id);
            await storage.Set(key, new NullContainer { Value = null });
            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Value.Should().BeNull();

            await storage.Set(key, new NullContainer { Value = new Car { Color = "red" } });
            existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Value.Should().NotBeNull();
            existing.Value.Value!.Color.Should().Be("red");
        }

        [Fact]
        public async Task WillSetAndGetPrimitiveInt()
        {
            var key = StorageKey<int>.Create("foo");
            await storage.Set(key, 4);
            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Should().Be(4);
        }

        [Fact]
        public async Task WillSetAndGetPrimitiveString()
        {
            var key = StorageKey<string>.Create("foo");
            await storage.Set(key, "red");
            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Should().Be("red");
        }

        [Fact]
        public async Task WillSetAndGetPrimitiveBool()
        {
            var key = StorageKey<bool>.Create("foo");
            await storage.Set(key, true);
            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Should().Be(true);
        }

        [Fact]
        public async Task WillSetAndGetNestedStorageKey()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id).Extend<Tire>("FL");
            await storage.Set(key, new Tire { Diameter = 10 });
            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Diameter.Should().Be(10);
        }

        [Fact]
        public async Task WillRespectStorageKeyNesting()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id).Extend<Tire>("FL");
            await storage.Set(key, new Tire { Diameter = 10 });
            var nonExistingKey = StorageKey<Tire>.Create("FL");
            var existing = await storage.Get(nonExistingKey);
            existing.IsSuccessful.Should().BeFalse();
        }


        [Fact]
        public async Task WillReturnNotFoundForMissingKey()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeFalse();
        }

        [Fact]
        public async Task WillCheckContainsKeyCorrectly()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            await storage.Set(key, new Car { Color = "red" });
            var existing = await storage.ContainsKey(key);
            existing.Should().BeTrue();

            var nonExistingId = Guid.NewGuid().ToString();
            var nonExistingKey = StorageKey<Car>.Create(nonExistingId);
            var nonExistingResult = await storage.ContainsKey(nonExistingKey);
            nonExistingResult.Should().BeFalse();
        }

        [Fact]
        public async Task WillCheckContainsNestedKeyCorrectly()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id).Extend<Tire>("FR");
            await storage.Set(key, new Tire { Diameter = 10 });
            var existing = await storage.ContainsKey(key);
            existing.Should().BeTrue();

            var nonExistingId = Guid.NewGuid().ToString();
            var nonExistingKey = StorageKey<Car>.Create(nonExistingId).Extend<Tire>("FR");
            var nonExistingResult = await storage.ContainsKey(nonExistingKey);
            nonExistingResult.Should().BeFalse();

            var nonExistingKey2 = StorageKey<Car>.Create(id).Extend<Tire>("FL");
            var nonExistingResult2 = await storage.ContainsKey(nonExistingKey);
            nonExistingResult2.Should().BeFalse();
        }

        [Fact]
        public async Task WillDeleteExisting()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            await storage.Set(key, new Car { Color = "red" });
            var result = await storage.Delete(key);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task WillReturnNotFoundWhenDeletingNonExisting()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var result = await storage.Delete(key);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task WillSetMany()
        {
            var ids = Enumerable.Range(0, 10)
                .Select(_ => Guid.NewGuid().ToString());
            var keys = ids.Select(StorageKey<Car>.Create);
            var pairs = keys.Select((key, i) => ((StorageKey)key, (object)new Car { Color = $"color_{i}" })).ToList();
            await storage.SetMany(pairs);

            for (int i = 0; i < pairs.Count; i++)
            {
                var (key, value) = pairs[i];
                var result = await storage.Get(key.As<Car>());
                result.IsSuccessful.Should().BeTrue();
                result.Value.Color.Should().Be($"color_{i}");
            }
        }

        [Fact]
        public async Task WillGetMany()
        {
            var ids = Enumerable.Range(0, 10)
                .Select(_ => Guid.NewGuid().ToString());
            var keys = ids.Select(StorageKey<Car>.Create).ToList();

            for (int i = 0; i < 10; i++)
            {
                var key = keys[i];
                await storage.Set(key, new Car { Color = $"color_{i}" });
            }

            var results = await storage.GetMany(keys.Cast<StorageKey>().ToList());
            results.Count.Should().Be(10);

            for (int i = 0; i < 10; i++)
            {
                var result = results[i];
                result.IsSuccessful.Should().BeTrue();
                result.Value.As<Car>().Color.Should().Be($"color_{i}");
            }
        }

        [Fact]
        public async Task WillGetManyTyped()
        {
            var ids = Enumerable.Range(0, 10)
                .Select(_ => Guid.NewGuid().ToString());
            var keys = ids.Select(StorageKey<Car>.Create).ToList();

            for (int i = 0; i < 10; i++)
            {
                var key = keys[i];
                await storage.Set(key, new Car { Color = $"color_{i}" });
            }

            var results = await storage.GetMany(keys);
            results.Count.Should().Be(10);

            for (int i = 0; i < 10; i++)
            {
                var result = results[i];
                result.IsSuccessful.Should().BeTrue();
                result.Value.Color.Should().Be($"color_{i}");
            }
        }

        [Fact]
        public async Task WillGetAndSetForeignKeys()
        {
            var carKey = StorageKey<Car>.Create(Guid.NewGuid().ToString());
            StorageKey<Car> carKey2 = StorageKey<Car>.Create(Guid.NewGuid().ToString());
            var tireKey = carKey.Extend<Tire>("FL");
            var manufacturerKey = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString());

            await storage.Set(carKey, new Car { Color = "red" }, [manufacturerKey.Extend<Car>()]);
            await storage.Set(carKey2, new Car { Color = "blue" }, [manufacturerKey.Extend<Car>()]);
            await storage.Set(tireKey, new Tire { Diameter = 10 }, [manufacturerKey.Extend<Tire>()]);

            var cars = await storage.GetManyByForeignKey(manufacturerKey.Extend<Car>());
            var tires = await storage.GetManyByForeignKey(manufacturerKey.Extend<Tire>());

            cars.Count.Should().Be(2);
            bool hasRed = false;
            bool hasBlue = false;

            foreach (var car in cars)
            {
                if (car.Value.Color.Equals("red")) hasRed = true;
                if (car.Value.Color.Equals("blue")) hasBlue = true;
            }

            hasRed.Should().BeTrue();
            hasBlue.Should().BeTrue();
            tires.Count.Should().Be(1);
            tires[0].Value.Diameter.Should().Be(10);
        }

        [Fact]
        public async Task WillDeleteByForeignKey()
        {
            var carKey = StorageKey<Car>.Create(Guid.NewGuid().ToString());
            var carKey2 = StorageKey<Car>.Create(Guid.NewGuid().ToString());
            var manufacturerKey = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString());

            await storage.Set(carKey, new Car { Color = "red" }, [manufacturerKey.Extend<Car>()]);
            await storage.Set(carKey2, new Car { Color = "blue" });

            var result = await storage.DeleteByForeignKey(manufacturerKey.Extend<Car>());
            result.Should().Be(1);

            var hasKey1 = await storage.ContainsKey(carKey);
            hasKey1.Should().BeFalse();
            var hasKey2 = await storage.ContainsKey(carKey2);
            hasKey2.Should().BeTrue();
        }

        [Fact]
        public async Task WillDeduplicateForeignKeyPrimaryKeyPairs()
        {
            var carKey = StorageKey<Car>.Create(Guid.NewGuid().ToString());
            var manufacturerKey = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString());

            await storage.Set(carKey, new Car { Color = "red" }, [manufacturerKey.Extend<Car>(), manufacturerKey.Extend<Car>()]);

            var result = await storage.GetManyByForeignKey(manufacturerKey.Extend<Car>());
            result.Count.Should().Be(1);
            result.Single().Value.Color.Should().Be("red");
        }

        [Fact]
        public async Task WillPageGetManyByForeignKey()
        {
            var carKey1 = StorageKey<Car>.Create("A" + Guid.NewGuid().ToString());
            var carKey2 = StorageKey<Car>.Create("B" + Guid.NewGuid().ToString());
            var carKey3 = StorageKey<Car>.Create("C" + Guid.NewGuid().ToString());
            var carKey4 = StorageKey<Car>.Create("D" + Guid.NewGuid().ToString());
            var manufacturerKey = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString()).Extend<Car>();

            await storage.Set(carKey1, new Car { Color = "red" }, [manufacturerKey]);
            await storage.Set(carKey2, new Car { Color = "blue" }, [manufacturerKey]);
            await storage.Set(carKey3, new Car { Color = "green" }, [manufacturerKey]);
            await storage.Set(carKey4, new Car { Color = "yellow" }, [manufacturerKey]);

            var page1Result = await storage.GetManyByForeignKey(manufacturerKey, 1);
            page1Result.Count.Should().Be(1);
            var page2Result = await storage.GetManyByForeignKey(manufacturerKey, 2, 1);
            page2Result.Count.Should().Be(2);
            var page3Result = await storage.GetManyByForeignKey(manufacturerKey, 1, 3);
            page3Result.Count.Should().Be(1);

            var result = page1Result.Concat(page2Result).Concat(page3Result)
                .DistinctBy(q => q.Key)
                .ToList();

            result.Count.Should().Be(4);
            result.Should().Contain(q => q.Value.Color == "red");
            result.Should().Contain(q => q.Value.Color == "blue");
            result.Should().Contain(q => q.Value.Color == "green");
            result.Should().Contain(q => q.Value.Color == "yellow");
        }

        [Fact]
        public async Task WillCountManyByForeignKey()
        {
            var carKey1 = StorageKey<Car>.Create("A" + Guid.NewGuid().ToString());
            var carKey2 = StorageKey<Car>.Create("B" + Guid.NewGuid().ToString());
            var carKey3 = StorageKey<Car>.Create("C" + Guid.NewGuid().ToString());
            var carKey4 = StorageKey<Car>.Create("D" + Guid.NewGuid().ToString());
            var manufacturerKey = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString()).Extend<Car>();
            var manufacturerKey2 = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString()).Extend<Car>();

            await storage.Set(carKey1, new Car { Color = "red" }, [manufacturerKey]);
            await storage.Set(carKey2, new Car { Color = "blue" });
            await storage.Set(carKey3, new Car { Color = "green" }, [manufacturerKey]);
            await storage.Set(carKey4, new Car { Color = "yellow" }, [manufacturerKey2]);

            var result = await storage.CountManyByForeignKey(manufacturerKey);
            result.Should().Be(2);
        }

        [Fact]
        public async Task WillAppendForeignKeys()
        {
            var carKey = StorageKey<Car>.Create(Guid.NewGuid().ToString());
            var manufacturerKey1 = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString()).Extend<Car>();
            var manufacturerKey2 = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString()).Extend<Car>();
            var manufacturerKey3 = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString()).Extend<Car>();

            await storage.Set(carKey, new Car { Color = "red" }, [manufacturerKey1]);
            await storage.Set(carKey, new Car { Color = "blue" }, [manufacturerKey2, manufacturerKey3]);

            var result = await storage.GetManyByForeignKey(manufacturerKey1);
            result.Count.Should().Be(1);
            result.Single().Value.Color.Should().Be("blue");
            var result2 = await storage.GetManyByForeignKey(manufacturerKey2);
            result2.Count.Should().Be(1);
            result2.Single().Value.Color.Should().Be("blue");
            var result3 = await storage.GetManyByForeignKey(manufacturerKey3);
            result3.Count.Should().Be(1);
            result3.Single().Value.Color.Should().Be("blue");
        }

        [Fact]
        public async Task WillDeleteForeignKeysWhenPrimaryKeyDeleted()
        {
            var carKey = StorageKey<Car>.Create(Guid.NewGuid().ToString());
            var manufacturerKey1 = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString()).Extend<Car>();
            var manufacturerKey2 = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString()).Extend<Car>();

            await storage.Set(carKey, new Car { Color = "red" }, [manufacturerKey1]);
            await storage.Set(carKey, new Car { Color = "blue" }, [manufacturerKey2]);
            await storage.Delete(carKey);

            var result = await storage.GetManyByForeignKey(manufacturerKey1);
            result.Count.Should().Be(0);
            var result2 = await storage.GetManyByForeignKey(manufacturerKey2);
            result2.Count.Should().Be(0);
        }

        [Fact]
        public async Task WillPerformTransactionalBatchSet()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            await storage.PerformTransactionalBatch(new List<StorageOperation>
            {
                new SetOperation
                {
                    Target = key,
                    Value = new Car { Color = "red" }
                },
                new SetOperation
                {
                    Target = key2,
                    Value = new Car { Color = "blue" }
                }
            });
            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("red");

            existing = await storage.Get(key2);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("blue");
        }

        [Fact]
        public async Task WillPerformTransactionalBatchAddForeignKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var foreignId = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            var foreignKey = StorageKey<Car>.Create(foreignId);
            await storage.Set(key, new Car { Color = "red" });
            await storage.Set(key3, new Car { Color = "green" });
            await storage.PerformTransactionalBatch(new List<StorageOperation>
            {
                new SetOperation
                {
                    Target = key2,
                    Value = new Car { Color = "blue" }
                },
                new AddForeignKeyOperation
                {
                    Target = key,
                    ForeignKey = foreignKey,
                },
                new AddForeignKeyOperation
                {
                    Target = key2,
                    ForeignKey = foreignKey,
                },
            });

            var cars = await storage.GetManyByForeignKey(foreignKey);
            cars.Count.Should().Be(2);
            bool hasRed = false;
            bool hasBlue = false;

            foreach (var car in cars)
            {
                if (car.Value.Color.Equals("red")) hasRed = true;
                if (car.Value.Color.Equals("blue")) hasBlue = true;
            }

            hasRed.Should().BeTrue();
            hasBlue.Should().BeTrue();
        }

        [Fact]
        public async Task WillPerformTransactionalBatchDelete()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            await storage.Set(key, new Car { Color = "red" });
            await storage.Set(key2, new Car { Color = "blue" });
            await storage.Set(key3, new Car { Color = "green" });
            await storage.PerformTransactionalBatch(new List<StorageOperation>
            {
                new DeleteOperation
                {
                    Target = key,
                },
                new DeleteOperation
                {
                    Target = key2,
                }
            });

            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeFalse();

            existing = await storage.Get(key2);
            existing.IsSuccessful.Should().BeFalse();

            existing = await storage.Get(key3);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("green");
        }

        [Fact]
        public async Task WillPerformTransactionalBatchDeleteByForeignKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var id4 = Guid.NewGuid().ToString();
            var fkid1 = Guid.NewGuid().ToString();
            var fkid2 = Guid.NewGuid().ToString();
            var fkey = StorageKey<Car>.Create(fkid1);
            var fkey2 = StorageKey<Car>.Create(fkid2);
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            var key4 = StorageKey<Car>.Create(id4);
            await storage.Set(key, new Car { Color = "red" }, [fkey]);
            await storage.Set(key2, new Car { Color = "blue" }, [fkey]);
            await storage.Set(key3, new Car { Color = "green" }, [fkey2]);
            await storage.Set(key4, new Car { Color = "yellow" });
            await storage.PerformTransactionalBatch(new List<StorageOperation>
            {
                new DeleteByForeignKeyOperation
                {
                    Target = fkey,
                }
            });

            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeFalse();

            existing = await storage.Get(key2);
            existing.IsSuccessful.Should().BeFalse();

            existing = await storage.Get(key3);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("green");

            existing = await storage.Get(key4);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("yellow");
        }

        [Fact]
        public async Task WillPerformTransactionalBatchDeleteForeignKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var id4 = Guid.NewGuid().ToString();
            var fkid1 = Guid.NewGuid().ToString();
            var fkid2 = Guid.NewGuid().ToString();
            var fkey = StorageKey<Car>.Create(fkid1);
            var fkey2 = StorageKey<Car>.Create(fkid2);
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            var key4 = StorageKey<Car>.Create(id4);
            await storage.Set(key, new Car { Color = "red" }, [fkey]);
            await storage.Set(key2, new Car { Color = "blue" }, [fkey]);
            await storage.Set(key3, new Car { Color = "green" }, [fkey2]);
            await storage.Set(key4, new Car { Color = "yellow" });
            await storage.PerformTransactionalBatch(new List<StorageOperation>
            {
                new DeleteForeignKeyOperation
                {
                    Target = fkey,
                }
            });

            var cars = await storage.GetManyByForeignKey(fkey);
            cars.Count.Should().Be(0);

            cars = await storage.GetManyByForeignKey(fkey2);
            cars.Count.Should().Be(1);
        }

        [Fact]
        public async Task WillRollbackTransactionalBatchSet()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            await storage.Set(key, new Car { Color = "red" });
            await storage.Invoking(s => s.PerformTransactionalBatch(new List<StorageOperation>
            {
                new SetOperation
                {
                    Target = key2,
                    Value = new Car { Color = "blue" }
                },
                new SetOperation
                {
                    Target = key3,
                    Value = new ErrorCar { Color = "green" }
                }
            })).Should().ThrowAsync<InvalidOperationException>();
            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("red");

            existing = await storage.Get(key2);
            existing.IsSuccessful.Should().BeFalse();
            existing = await storage.Get(key3);
            existing.IsSuccessful.Should().BeFalse();
        }

        [Fact]
        public async Task WillPerformTransactionalBatchAdd()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            await storage.Set(key2, new Car { Color = "green" });
            await storage.PerformTransactionalBatch(new List<StorageOperation>
            {
                new AddOperation
                {
                    Target = key,
                    Value = new Car { Color = "blue" }
                },
            });

            var car = await storage.Get(key);
            var car2 = await storage.Get(key2);
            car.IsSuccessful.Should().BeTrue();
            car.Value.Color.Should().BeEquivalentTo("blue");
            car2.IsSuccessful.Should().BeTrue();
            car2.Value.Color.Should().BeEquivalentTo("green");
        }

        [Fact]
        public async Task WillThrowExceptionOnConflictingTransactionBatchAdd()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            await storage.Set(key, new Car { Color = "green" });
            await storage.Invoking(s => s.PerformTransactionalBatch(new List<StorageOperation>
            {
                new AddOperation
                {
                    Target = key,
                    Value = new Car { Color = "blue" }
                },
            })).Should().ThrowAsync<StorageKeyExistsException>();
        }

        [Fact]
        public async Task WillPerformTransactionalBatchRemoveForeignKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var foreignId = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var foreignKey = StorageKey<Car>.Create(foreignId);
            await storage.Set(key, new Car { Color = "red" }, [foreignKey]);
            await storage.Set(key2, new Car { Color = "green" }, [foreignKey]);
            await storage.PerformTransactionalBatch(new List<StorageOperation>
            {
                new RemoveForeignKeyOperation
                {
                    Target = key,
                    ForeignKey = foreignKey,
                },
            });

            var cars = await storage.GetManyByForeignKey(foreignKey);
            cars.Count.Should().Be(1);
            cars.Any(c => c.Value.Color == "red").Should().BeFalse();
            cars.Any(c => c.Value.Color == "green").Should().BeTrue();
        }

        [Fact]
        public async Task WillRollbackTransactionalBatchAddForeignKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var foreignId = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            var foreignKey = StorageKey<Car>.Create(foreignId);
            await storage.Set(key, new Car { Color = "red" });
            await storage.Set(key3, new Car { Color = "green" });
            await storage.Invoking(s => s.PerformTransactionalBatch(new List<StorageOperation>
            {
                new SetOperation
                {
                    Target = key2,
                    Value = new Car { Color = "blue" }
                },
                new AddForeignKeyOperation
                {
                    Target = key,
                    ForeignKey = foreignKey,
                },
                new AddForeignKeyOperation
                {
                    Target = key2,
                    ForeignKey = foreignKey,
                },
                new SetOperation
                {
                    Target = key3,
                    Value = new ErrorCar { Color = "green" }
                }
            })).Should().ThrowAsync<InvalidOperationException>();

            var cars = await storage.GetManyByForeignKey(foreignKey);
            cars.Count.Should().Be(0);
        }

        [Fact]
        public async Task WillRollbackTransactionalBatchRemoveForeignKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var foreignId = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var foreignKey = StorageKey<Car>.Create(foreignId);
            await storage.Set(key, new Car { Color = "red" }, [foreignKey]);
            await storage.Set(key2, new Car { Color = "green" }, [foreignKey]);
            await storage.Invoking(s => s.PerformTransactionalBatch(new List<StorageOperation>
            {
                new SetOperation
                {
                    Target = key2,
                    Value = new Car { Color = "blue" }
                },
                new RemoveForeignKeyOperation
                {
                    Target = key,
                    ForeignKey = foreignKey,
                },
                new SetOperation
                {
                    Target = key2,
                    Value = new ErrorCar { Color = "green" }
                }
            })).Should().ThrowAsync<InvalidOperationException>();

            var cars = await storage.GetManyByForeignKey(foreignKey);
            cars.Count.Should().Be(2);
            cars.Any(c => c.Value.Color == "red").Should().BeTrue();
            cars.Any(c => c.Value.Color == "green").Should().BeTrue();
        }

        [Fact]
        public async Task WillRollbackTransactionalBatchAddKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            await storage.Set(key2, new Car { Color = "green" });
            await storage.Invoking(s => s.PerformTransactionalBatch(new List<StorageOperation>
            {
                new AddOperation
                {
                    Target = key,
                    Value = new Car { Color = "blue" }
                },
                new SetOperation
                {
                    Target = key2,
                    Value = new ErrorCar { Color = "blue" }
                },
            })).Should().ThrowAsync<InvalidOperationException>();

            var car = await storage.Get(key);
            var car2 = await storage.Get(key2);
            car.IsSuccessful.Should().BeFalse();
            car2.IsSuccessful.Should().BeTrue();
            car2.Value.Color.Should().BeEquivalentTo("green");
        }

        [Fact]
        public async Task WillRollbackTransactionalBatchDelete()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            await storage.Set(key, new Car { Color = "red" });
            await storage.Set(key2, new Car { Color = "blue" });
            await storage.Set(key3, new Car { Color = "green" });
            await storage.Invoking(s => s.PerformTransactionalBatch(new List<StorageOperation>
            {
                new DeleteOperation
                {
                    Target = key,
                },
                new DeleteOperation
                {
                    Target = key2,
                },
                new SetOperation
                {
                    Target = key3,
                    Value = new ErrorCar { Color = "green" }
                }
            })).Should().ThrowAsync<InvalidOperationException>();

            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("red");

            existing = await storage.Get(key2);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("blue");

            existing = await storage.Get(key3);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("green");
        }

        [Fact]
        public async Task WillRollbackTransactionalBatchDeleteByForeignKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var fkid1 = Guid.NewGuid().ToString();
            var fkey = StorageKey<Car>.Create(fkid1);
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            await storage.Set(key, new Car { Color = "red" }, [fkey]);
            await storage.Set(key2, new Car { Color = "blue" }, [fkey]);
            await storage.Invoking(s => s.PerformTransactionalBatch(new List<StorageOperation>
            {
                new DeleteByForeignKeyOperation
                {
                    Target = fkey,
                },
                new SetOperation
                {
                    Target = key,
                    Value = new ErrorCar { Color = "green" }
                }
            })).Should().ThrowAsync<InvalidOperationException>();

            var existing = await storage.Get(key);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("red");

            existing = await storage.Get(key2);
            existing.IsSuccessful.Should().BeTrue();
            existing.Value.Color.Should().Be("blue");
        }

        [Fact]
        public async Task WillRollbackTransactionalBatchDeleteForeignKey()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var id4 = Guid.NewGuid().ToString();
            var fkid1 = Guid.NewGuid().ToString();
            var fkid2 = Guid.NewGuid().ToString();
            var fkey = StorageKey<Car>.Create(fkid1);
            var fkey2 = StorageKey<Car>.Create(fkid2);
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            var key4 = StorageKey<Car>.Create(id4);
            await storage.Set(key, new Car { Color = "red" }, [fkey]);
            await storage.Set(key2, new Car { Color = "blue" }, [fkey]);
            await storage.Set(key3, new Car { Color = "green" }, [fkey2]);
            await storage.Set(key4, new Car { Color = "yellow" });
            await storage.Invoking(s => s.PerformTransactionalBatch(new List<StorageOperation>
            {
                new DeleteForeignKeyOperation
                {
                    Target = fkey,
                },
                new SetOperation
                {
                    Target = key,
                    Value = new ErrorCar { Color = "green" }
                }
            })).Should().ThrowAsync<InvalidOperationException>();

            var cars = await storage.GetManyByForeignKey(fkey);
            cars.Count.Should().Be(2);
            cars.Count.Should().Be(2);
            bool hasRed = false;
            bool hasBlue = false;
            foreach (var car in cars)
            {
                if (car.Value.Color.Equals("red")) hasRed = true;
                if (car.Value.Color.Equals("blue")) hasBlue = true;
            }
            hasRed.Should().BeTrue();
            hasBlue.Should().BeTrue();

            cars = await storage.GetManyByForeignKey(fkey2);
            cars.Count.Should().Be(1);
        }

        [Fact]
        public async Task WillRetrieveForeignKeys()
        {
            var id = Guid.NewGuid().ToString();
            var id2 = Guid.NewGuid().ToString();
            var id3 = Guid.NewGuid().ToString();
            var id4 = Guid.NewGuid().ToString();
            var fkid1 = Guid.NewGuid().ToString();
            var fkid2 = Guid.NewGuid().ToString();
            var fkey = StorageKey<Car>.Create(fkid1);
            var fkey2 = StorageKey<Car>.Create(fkid2);
            var key = StorageKey<Car>.Create(id);
            var key2 = StorageKey<Car>.Create(id2);
            var key3 = StorageKey<Car>.Create(id3);
            var key4 = StorageKey<Car>.Create(id4);
            await storage.Set(key, new Car { Color = "red" }, [fkey]);
            await storage.Set(key2, new Car { Color = "blue" }, [fkey, fkey2]);
            await storage.Set(key4, new Car { Color = "yellow" });

            var set1 = await storage.GetForeignKeys(key);
            set1.Should().HaveCount(1); ;
            set1.Single().SingleValue().Should().Be(fkid1);

            var set2 = await storage.GetForeignKeys(key2);
            set2.Should().HaveCount(2); ;
            var ids = set2.Select(sk => sk.SingleValue()).ToList();
            ids.Should().Contain(fkid1);
            ids.Should().Contain(fkid2);

            var set3 = await storage.GetForeignKeys(key3);
            set3.Should().BeEmpty();
            var set4 = await storage.GetForeignKeys(key4);
            set4.Should().BeEmpty();
        }
    }
}
