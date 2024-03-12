import AWS from "aws-sdk";

export let dynamodb;
const TABLE_NAME = "GameScores";
const createTables = () => {};

export const connectToDB = async () => {
  console.log(
    "Credentials:",
    process.env.AWS_ACCESS_KEY_ID,
    process.env.AWS_SECRET_ACCESS_KEY,
  );
  AWS.config.update({
    region: "eu-west-2",
    accessKeyId: process.env.AWS_ACCESS_KEY_ID,
    secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY,
  });

  return new Promise((resolve, reject) => {
    dynamodb = new AWS.DynamoDB();

    dynamodb.listTables({}, (err, data) => {
      if (err) {
        console.log(err);
      } else {
        console.log("Connected to DynamoDB");
        console.log("Tables: ", data.TableNames);
        resolve("Connected to DynamoDB");
      }
    });
  });
};

export const insertPlayerIntoDB = async (
  gameId: number,
  playerId: string,
  playerTag: string,
  curSize: number,
) => {
  const params = {
    TableName: "GameScores",
    Item: {
      gameId: { N: gameId.toString() },
      playerId: { S: playerId },
      playerTag: { S: playerTag },
      size: { N: curSize.toString() },
      alive: { BOOL: true },
    },
  };

  let data = await dynamodb.putItem(params).promise();
  console.log("Inserted new player to dynamoDB: ", data);
  return;
};

// export const UpdatePlayersScore;

export const DeletePlayersByGameId = async (gameId: number) => {
  // Use the query method to retrieve all items with the specified partition key

  let results = await dynamodb
    .query({
      TableName: TABLE_NAME,
      KeyConditionExpression: "#pk = :pk",
      ExpressionAttributeNames: {
        "#pk": "gameId",
      },
      ExpressionAttributeValues: {
        ":pk": { N: gameId.toString() },
      },
    })
    .promise();

  let items = results.Items;
  if (items.length == 0) return;
  // console.log("Items:", items);

  let deleteRequests = [];
  for (let i = 0; i < items.length; i++) {
    // console.log("Game id:", items[i].gameId);
    // console.log("Player id:", items[i].playerId);
    let res = await dynamodb
      .deleteItem({
        TableName: TABLE_NAME,
        Key: {
          gameId: items[i].gameId,
          playerId: items[i].playerId,
        },
      })
      .promise();
  }

  // deleteRequests.push({
  //   DeleteRequest: {
  //     Key: {
  //       gameId: items[i].gameId,
  //       playerId: items[i].playerId,
  //     },
  //   },
  // });
  // await dynamodb.batchWriteItem({
  //   RequestItems: {
  //     [TABLE_NAME]: deleteRequests,
  //   },
  // });
};

export const UpdatePlayerSize = async (
  gameId: number,
  playerId: string,
  size: number,
) => {
  let res = await dynamodb
    .updateItem({
      TableName: TABLE_NAME,
      Key: {
        gameId: { N: gameId.toString() },
        playerId: { S: playerId },
      },
      UpdateExpression: "set #size = :size",
      ExpressionAttributeNames: {
        "#size": "size",
      },
      ExpressionAttributeValues: {
        ":size": { N: size.toString() },
      },
    })
    .promise();
  console.log("Updated player in dynamo:", res);
};
