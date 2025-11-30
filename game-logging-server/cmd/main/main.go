package main

import (
	"context"
	"net/http"
	"os"
	"time"

	"github.com/gin-gonic/gin"
	"go.mongodb.org/mongo-driver/v2/bson"
	"go.mongodb.org/mongo-driver/v2/mongo"
	"go.mongodb.org/mongo-driver/v2/mongo/options"
)

// LogEntry represents a log record
type LogEntry struct {
	User string `json:"user" bson:"user"`
	Data string `json:"data" bson:"data"`
}

// getIP extracts client IP from request
func getIP(c *gin.Context) string {
	ip := c.ClientIP()
	return ip
}

func writeGameInfo(c *gin.Context) {
	var entry LogEntry

	// Bind JSON body
	if err := c.ShouldBindJSON(&entry); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"message": "invalid input", "error": err.Error()})
		return
	}

	// Defaults
	if entry.User == "" {
		entry.User = "unknown"
	}

	if entry.Data == "start" {
		entry.Data = "login;" + getIP(c)
	}

	mongoURI := os.Getenv("MONGO_URI")

	dbName := "GameLogs"

	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	client, err := mongo.Connect(options.Client().ApplyURI(mongoURI))
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"message": "failed to connect to MongoDB", "error": err.Error()})
		return
	}
	defer client.Disconnect(ctx)

	collection := client.Database(dbName).Collection("game_log")

	// Insert log entry
	_, err = collection.InsertOne(ctx, bson.M{
		"user": entry.User,
		"data": entry.Data,
	})
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"message": "failed to insert log", "error": err.Error()})
		return
	}

	// Successful response
	c.JSON(http.StatusCreated, gin.H{})
}

func main() {
	r := gin.Default()
	r.POST("/recordGameLog", writeGameInfo)
	r.Run(":8080")
}
