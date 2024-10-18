haondt = db.getSiblingDB('haondt');
haondt.createCollection('haondt');

haondt.haondt.createIndex({ "PrimaryKey": 1 }, { unique: true });
haondt.haondt.createIndex({ "ForeignKeys": 1 });

print("Indexes created on 'haondt' collection:");
printjson(haondt.haondt.getIndexes());
