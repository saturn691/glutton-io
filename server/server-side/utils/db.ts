export let dynamodb;

import { MongoClient } from "mongodb";
export let mongoCli: MongoClient;

/**
 * Called at the start of the server to connect to the database.
 * Initialises mongo client and connects to the database.
 */
export const connectToDB = async () => {
  mongoCli = new MongoClient(process.env.MONGO_URI, {});
  await mongoCli.connect();
  console.log("Connected to MongoDB");
};

/**
 * Inserts a player into the database.
 * @param gameId the id of the game
 * @param socketId the socket id of the player
 * @param playerTag the tag of the player
 * @param size the size of the player
 * @returns a promise that resolves when the player is inserted
 */
export const insertPlayerIntoDB = async (
  gameId: number,
  socketId: string,
  playerTag: string,
  size: number
) => {
  const player = {
    gameId,
    socketId,
    playerTag,
    size,
  };

  try {
    const db = mongoCli.db("gluttonio");
    const playersCollection = db.collection("players");
    await playersCollection.insertOne(player);
    console.log("Player inserted into MongoDB");
  } catch (error) {
    console.error("Error inserting player into MongoDB:", error);
  }
};

/**
 * Updates the size of a player in the database.
 * @param gameId the id of the game
 * @param socketId the socket id of the player
 * @param size the new size of the player
 * @returns a promise that resolves when the update operation is complete
 */
export const updatePlayerSize = async (
  gameId: number,
  socketId: string,
  size: number
) => {
  try {
    const db = mongoCli.db("gluttonio");
    const playersCollection = db.collection("players");
    await playersCollection.updateOne(
      { socketId: socketId, gameId: gameId },
      { $set: { size: size } }
    );
    // console.log("Player size updated in MongoDB");
  } catch (error) {
    console.error("Error updating player size in MongoDB:", error);
  }
};

/**
 * Gets top N players by size from the database.
 * @param gameId the id of the game
 * @param n the number of players to return
 * @returns a promise that resolves when the read operation is complete
 */
export const getTopNPlayersBySize = async (gameId: number, n: number) => {
  const db = mongoCli.db("gluttonio");
  const playersCollection = db.collection("players");

  try {
    const topPlayers = await playersCollection
      .find({})
      .sort({ size: -1 })
      .limit(5)
      .project({ _id: 0, socketId: 1, size: 1 })
      .toArray();
    return topPlayers;
  } catch (error) {
    console.error("Error retrieving top 5 players from MongoDB:", error);
  }
};

/**
 * Deletes players from the database by socket id.
 * @param gameId the id of the game
 * @param socketId the socket id of the player
 * @returns a promise that resolves when the delete operation is complete
 */
export const deletePlayerBySocketId = async (
  gameId: number,
  socketId: string
) => {
  const db = mongoCli.db("gluttonio");
  const playersCollection = db.collection("players");
  try {
    await playersCollection.deleteOne({ gameId: gameId, socketId: socketId });
    console.log("Deleted player from MongoDB");
  } catch (error) {
    console.log("Error deleting players from MongoDB:", error);
  }
};

/**
 * Deletes all players from the database by game id.
 * @param gameId the id of the game
 * @returns a promise that resolves when the delete operation is complete
 */
export const deletePlayersByGameId = async (gameId: number) => {
  const db = mongoCli.db("gluttonio");
  const playersCollection = db.collection("players");
  try {
    await playersCollection.deleteMany({ gameId: 1 });
    console.log("Deleted all players from MongoDB");
  } catch (error) {
    console.log("Error deleting players from MongoDB:", error);
  }
};
