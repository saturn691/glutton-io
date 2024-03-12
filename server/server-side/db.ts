import AWS from 'aws-sdk'

export let dynamodb;

const createTables = () => {

}

export const connectToDB = async () => {
    AWS.config.update({
        region: 'eu-west-2',
        accessKeyId: process.env.AWS_ACCESS_KEY_ID,
        secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY
    })  

    return new Promise((resolve, reject) => {
        dynamodb = new AWS.DynamoDB();
        
        dynamodb.listTables({}, (err, data) => {
        if (err) {
            console.log(err)
        } else {
            console.log('Connected to DynamoDB')
            console.log("Tables: ", data)
            resolve("Connected to DynamoDB")
        }
    })
    })  
}