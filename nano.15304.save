{
  "openapi": "3.0.1",
  "info": {
    "title": "gRPC transcoding",
    "version": "v1"
  },
  "paths": {
    "/api/auth/register": {
      "post": {
        "tags": [
          "Authentication"
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/RegisterRequest"
              }
            }
          }
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/RegisterResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/auth/login": {
      "post": {
        "tags": [
          "Authentication"
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/LoginRequest"
              }
            }
          }
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/LoginResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{votingId}/candidates": {
      "post": {
        "tags": [
          "Candidates"
        ],
        "parameters": [
          {
            "name": "votingId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/AddCandidateRequest"
              }
            }
          }
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/CandidateResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      },
      "get": {
        "tags": [
          "Candidates"
        ],
        "parameters": [
          {
            "name": "votingId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/GetCandidatesResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{votingId}/candidates/{candidateId}": {
      "delete": {
        "tags": [
          "Candidates"
        ],
        "parameters": [
          {
            "name": "votingId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          },
          {
            "name": "candidateId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Empty"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/candidates/{candidateId}/photo": {
      "post": {
        "tags": [
          "Candidates"
        ],
        "parameters": [
          {
            "name": "candidateId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/UploadCandidatePhotoRequest"
              }
            }
          }
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/UploadCandidatePhotoResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/users": {
      "get": {
        "tags": [
          "Users"
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/GetUsersResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings": {
      "post": {
        "tags": [
          "Votings"
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/CreateVotingRequest"
              }
            }
          }
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/VotingResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      },
      "get": {
        "tags": [
          "Votings"
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/GetVotingsResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{id}": {
      "put": {
        "tags": [
          "Votings"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/UpdateVotingRequest"
              }
            }
          }
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/VotingResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      },
      "delete": {
        "tags": [
          "Votings"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Empty"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      },
      "get": {
        "tags": [
          "Votings"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/VotingResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{id}/start": {
      "post": {
        "tags": [
          "Votings"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Empty"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{id}/pause": {
      "post": {
        "tags": [
          "Votings"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Empty"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{id}/finish": {
      "post": {
        "tags": [
          "Votings"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Empty"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{votingId}/vote": {
      "post": {
        "tags": [
          "Votings"
        ],
        "parameters": [
          {
            "name": "votingId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/CastVoteRequest"
              }
            }
          }
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/CastVoteResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{id}/results": {
      "get": {
        "tags": [
          "Votings"
        ],
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/VotingResultsResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{votingId}/targets": {
      "post": {
        "tags": [
          "VotingTargets"
        ],
        "parameters": [
          {
            "name": "votingId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "requestBody": {
          "content": {
            "application/json": {
              "schema": {
                "$ref": "#/components/schemas/AddVotingTargetRequest"
              }
            }
          }
        },
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/VotingTargetResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      },
      "get": {
        "tags": [
          "VotingTargets"
        ],
        "parameters": [
          {
            "name": "votingId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/GetVotingTargetsResponse"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    },
    "/api/votings/{votingId}/targets/{educationUnitId}": {
      "delete": {
        "tags": [
          "VotingTargets"
        ],
        "parameters": [
          {
            "name": "votingId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          },
          {
            "name": "educationUnitId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string"
            }
          }
        ],
        "responses": {
          "200": {
            "description": "OK",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Empty"
                }
              }
            }
          },
          "default": {
            "description": "Error",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/Status"
                }
              }
            }
          }
        }
      }
    }
  },
  "components": {
    "schemas": {
      "AddCandidateRequest": {
        "type": "object",
        "properties": {
          "votingId": {
            "type": "string"
          },
          "name": {
            "type": "string"
          },
          "description": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "AddVotingTargetRequest": {
        "type": "object",
        "properties": {
          "votingId": {
            "type": "string"
          },
          "educationUnitId": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "Any": {
        "required": [
          "@type"
        ],
        "type": "object",
        "properties": {
          "@type": {
            "type": "string"
          }
        },
        "additionalProperties": { }
      },
      "CandidateResponse": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string"
          },
          "votingId": {
            "type": "string"
          },
          "name": {
            "type": "string"
          },
          "description": {
            "type": "string"
          },
          "photoUrl": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "CastVoteRequest": {
        "type": "object",
        "properties": {
          "votingId": {
            "type": "string"
          },
          "userId": {
            "type": "string"
          },
          "selectedCandidateId": {
            "type": "string"
          },
          "selectedCandidateIds": {
            "type": "array",
            "items": {
              "type": "string"
            }
          },
          "ratingAnswers": {
            "type": "object",
            "additionalProperties": {
              "type": "integer",
              "format": "int32"
            }
          },
          "textAnswer": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "CastVoteResponse": {
        "type": "object",
        "properties": {
          "voteId": {
            "type": "string"
          },
          "voteHash": {
            "type": "string"
          },
          "createdAt": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "CreateVotingRequest": {
        "type": "object",
        "properties": {
          "title": {
            "type": "string"
          },
          "description": {
            "type": "string"
          },
          "type": {
            "$ref": "#/components/schemas/VotingType"
          },
          "isAnonymous": {
            "type": "boolean"
          },
          "allowVoteChange": {
            "type": "boolean"
          },
          "startTime": {
            "type": "string"
          },
          "endTime": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "Empty": {
        "type": "object",
        "additionalProperties": false
      },
      "GetCandidatesResponse": {
        "type": "object",
        "properties": {
          "candidates": {
            "type": "array",
            "items": {
              "$ref": "#/components/schemas/CandidateResponse"
            }
          }
        },
        "additionalProperties": false
      },
      "GetUsersResponse": {
        "type": "object",
        "properties": {
          "users": {
            "type": "array",
            "items": {
              "$ref": "#/components/schemas/UserResponse"
            }
          }
        },
        "additionalProperties": false
      },
      "GetVotingTargetsResponse": {
        "type": "object",
        "properties": {
          "targets": {
            "type": "array",
            "items": {
              "$ref": "#/components/schemas/VotingTargetResponse"
            }
          }
        },
        "additionalProperties": false
      },
      "GetVotingsResponse": {
        "type": "object",
        "properties": {
          "votings": {
            "type": "array",
            "items": {
              "$ref": "#/components/schemas/VotingResponse"
            }
          }
        },
        "additionalProperties": false
      },
      "LoginRequest": {
        "type": "object",
        "properties": {
          "email": {
            "type": "string"
          },
          "password": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "LoginResponse": {
        "type": "object",
        "properties": {
          "userId": {
            "type": "string"
          },
          "role": {
            "type": "string"
          },
          "accessToken": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "RegisterRequest": {
        "type": "object",
        "properties": {
          "email": {
            "type": "string"
          },
          "password": {
            "type": "string"
          },
          "name": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "RegisterResponse": {
        "type": "object",
        "properties": {
          "userId": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "Status": {
        "type": "object",
        "properties": {
          "code": {
            "type": "integer",
            "format": "int32"
          },
          "message": {
            "type": "string"
          },
          "details": {
            "type": "array",
            "items": {
              "$ref": "#/components/schemas/Any"
            }
          }
        },
        "additionalProperties": false
      },
      "Struct": {
        "type": "object",
        "additionalProperties": { }
      },
      "UpdateVotingRequest": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string"
          },
          "title": {
            "type": "string"
          },
          "description": {
            "type": "string"
          },
          "type": {
            "$ref": "#/components/schemas/VotingType"
          },
          "isAnonymous": {
            "type": "boolean"
          },
          "allowVoteChange": {
            "type": "boolean"
          },
          "startTime": {
            "type": "string"
          },
          "endTime": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "UploadCandidatePhotoRequest": {
        "type": "object",
        "properties": {
          "candidateId": {
            "type": "string"
          },
          "imageBase64": {
            "type": "string"
          },
          "contentType": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "UploadCandidatePhotoResponse": {
        "type": "object",
        "properties": {
          "photoUrl": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "UserResponse": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string"
          },
          "email": {
            "type": "string"
          },
          "role": {
            "type": "string"
          },
          "createdAt": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "VotingResponse": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string"
          },
          "title": {
            "type": "string"
          },
          "description": {
            "type": "string"
          },
          "type": {
            "$ref": "#/components/schemas/VotingType"
          },
          "isAnonymous": {
            "type": "boolean"
          },
          "allowVoteChange": {
            "type": "boolean"
          },
          "startTime": {
            "type": "string"
          },
          "endTime": {
            "type": "string"
          },
          "status": {
            "$ref": "#/components/schemas/VotingStatus"
          },
          "createdAt": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "VotingResultsResponse": {
        "type": "object",
        "properties": {
          "votingId": {
            "type": "string"
          },
          "results": {
            "type": "object",
            "additionalProperties": {
              "$ref": "#/components/schemas/Struct"
            }
          },
          "resultHash": {
            "type": "string"
          },
          "calculatedAt": {
            "type": "string"
          },
          "totalVotes": {
            "type": "integer",
            "format": "int32"
          }
        },
        "additionalProperties": false
      },
      "VotingStatus": {
        "enum": [
          "VotingStatusUnspecified",
          "Draft",
          "Active",
          "Paused",
          "Finished"
        ],
        "type": "string"
      },
      "VotingTargetResponse": {
        "type": "object",
        "properties": {
          "votingId": {
            "type": "string"
          },
          "educationUnitId": {
            "type": "string"
          }
        },
        "additionalProperties": false
      },
      "VotingType": {
        "enum": [
          "VotingTypeUnspecified",
          "SingleChoice",
          "MultipleChoice",
          "Rating",
          "OpenAnswer"
        ],
        "type": "string"
      }
    }
  }
}
