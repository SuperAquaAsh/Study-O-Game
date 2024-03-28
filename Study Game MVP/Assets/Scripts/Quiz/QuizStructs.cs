using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

[Serializable]
public struct Answer : INetworkSerializable{
    public string answerText;
    public bool isRight;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref answerText);
        serializer.SerializeValue(ref isRight);
    }
}

[Serializable]
public struct Question : INetworkSerializable{
    public string questionText;
    public Answer[] answers;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref questionText);
        serializer.SerializeValue(ref answers);
    }
}

[Serializable]
public struct Quiz : INetworkSerializable{
    public uint quizCode;
    public string quizName;
    public Question[] questions;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref quizCode);
        serializer.SerializeValue(ref quizName);
        serializer.SerializeValue(ref questions);
    }
}


public struct SavedQuiz{
    public string quizName;
    public string saveLocation;
    public uint quizCode;
}
