using FluentAssertions;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;

namespace Haondt.Persistence.Tests
{
    public class Car
    {
        public required string Color { get; set; }
    }

    public class Manufacturer
    {
        public required string Name { get; set; }
    }

    public class Tire
    {
        public required int Diameter { get; set; }
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
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task WillReturnNotFoundWhenDeletingNonExisting()
        {
            var id = Guid.NewGuid().ToString();
            var key = StorageKey<Car>.Create(id);
            var result = await storage.Delete(key);
            result.IsSuccessful.Should().BeFalse();
            result.Reason.Should().Be(StorageResultReason.NotFound);
        }

        [Fact]
        public async Task WillSetMany()
        {
            var ids = Enumerable.Range(0, 10)
                .Select(_ => Guid.NewGuid().ToString());
            var keys = ids.Select(StorageKey<Car>.Create);
            var pairs = keys.Select((key, i) => ((StorageKey)key, (object?)new Car { Color = $"color_{i}" })).ToList();
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
            StorageKey<Car>? carKey2 = StorageKey<Car>.Create(Guid.NewGuid().ToString());
            var tireKey = carKey.Extend<Tire>("FL");
            var manufacturerKey = StorageKey<Manufacturer>.Create(Guid.NewGuid().ToString());

            await storage.Set(carKey, new Car { Color = "red" }, [manufacturerKey.Extend<Car>()]);
            await storage.Set(carKey2, new Car { Color = "blue" }, [manufacturerKey.Extend<Car>()]);
            await storage.Set(tireKey, new Tire { Diameter = 10 }, [manufacturerKey.Extend<Tire>()]);

            var cars = await storage.GetMany(manufacturerKey.Extend<Car>());
            var tires = await storage.GetMany(manufacturerKey.Extend<Tire>());

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

            var result = await storage.DeleteMany(manufacturerKey.Extend<Car>());
            result.IsSuccessful.Should().BeTrue();
            result.Value.Should().Be(1);

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

            var result = await storage.GetMany(manufacturerKey.Extend<Car>());
            result.Count.Should().Be(1);
            result.Single().Value.Color.Should().Be("red");
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

            var result = await storage.GetMany(manufacturerKey1);
            result.Count.Should().Be(1);
            result.Single().Value.Color.Should().Be("blue");
            var result2 = await storage.GetMany(manufacturerKey2);
            result2.Count.Should().Be(1);
            result2.Single().Value.Color.Should().Be("blue");
            var result3 = await storage.GetMany(manufacturerKey3);
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

            var result = await storage.GetMany(manufacturerKey1);
            result.Count.Should().Be(0);
            var result2 = await storage.GetMany(manufacturerKey2);
            result2.Count.Should().Be(0);
        }
    }
}
