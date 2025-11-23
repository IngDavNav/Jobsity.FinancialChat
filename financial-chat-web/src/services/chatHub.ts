import * as signalR from "@microsoft/signalr";
import type { ChatMessage } from "../types/ChatMessage";
import { API_BASE_URL } from "../config/api";


export function createChatConnection(
  onMessage: (msg: ChatMessage) => void
) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/chatHub`)
    .withAutomaticReconnect()
    .build();

  connection.on("ReceiveMessage", (message: ChatMessage) => {
    console.log("Mensaje recibido desde hub:", message);
    onMessage(message);
  });

  return connection;
}
