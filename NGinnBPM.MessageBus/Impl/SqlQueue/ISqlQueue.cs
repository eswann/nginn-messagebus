﻿/*
 * Created by SharpDevelop.
 * User: Rafal
 * Date: 2014-08-05
 * Time: 20:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data;
using System.Collections.Generic;

namespace NGinnBPM.MessageBus.Impl.SqlQueue
{
	public enum MessageFailureDisposition
    {
        Fail,
        RetryIncrementRetryCount,
        RetryDontIncrementRetryCount
    }

	/// <summary>
	/// Abstraction of sql queue operations
	/// </summary>
	public interface ISqlQueue
	{
		/// <summary>
		/// Inserts messages to message queue tables. Messages are passed in a dictionary where table name is the key
		/// and the value contains a list of messages to be inserted into that table 
		/// </summary>
		/// <param name="conn">database connection</param>
		/// <param name="messages">map: table name :: list of messages to insert into that table</param>
		void InsertMessageBatchToLocalDatabaseQueues(IDbConnection conn, IDictionary<string, ICollection<MessageContainer>> messages);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="conn">database connection</param>
		/// <param name="queueTable">queue table name</param>
		/// <param name="skipMessageIds">list of currently processed messsage ids that should be skipped when selecting 
		/// next message. If the database supports 'skip locked rows' this list can be ignored and database locking
		/// mechanism should be used instead.</param>
		/// <param name="retryTime"></param>
		/// <param name="moreMessages"></param>
		/// <returns></returns>
		MessageContainer SelectAndLockNextInputMessage(IDbConnection conn, string queueTable, Func<IEnumerable<string>> skipMessageIds, out DateTime? retryTime, out bool moreMessages);
		/// <summary>
		/// Mark message as handled. TODO (check if needed at all)
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="queueTable"></param>
		/// <param name="messageId"></param>
		void MarkMessageHandled(IDbConnection conn, string queueTable, string messageId);
		
		/// <summary>
		/// Move a single message from a retry (R) queue into input.
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="queueTable"></param>
		/// <param name="messageId"></param>
		/// <returns>true if message was moved, false otherwise</returns>
		bool MoveMessageFromRetryToInput(IDbConnection conn, string queueTable, string messageId);
		
		/// <summary>
		/// Schedule message for processing later.
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="queueTable"></param>
		/// <param name="messageId"></param>
		/// <param name="retryTime"></param>
		void MarkMessageForProcessingLater(IDbConnection conn, string queueTable, string messageId, DateTime? retryTime);
		
		/// <summary>
		/// Handle message failure - re-schedule for processing later or mark as permanent failure
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="queueTable"></param>
		/// <param name="messageId"></param>
		/// <param name="errorInfo"></param>
		/// <param name="disp"></param>
		/// <param name="retryTime"></param>
		/// <returns></returns>
		bool MarkMessageFailed(IDbConnection conn, string queueTable, string messageId, string errorInfo, MessageFailureDisposition disp, DateTime retryTime);
		
		/// <summary>
		/// Periodic cleanup of already processed messages
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="queueTable"></param>
		void CleanupProcessedMessages(IDbConnection conn, string queueTable, DateTime? olderThan);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="queueTable"></param>
		/// <returns></returns>
		bool MoveScheduledMessagesToInputQueue(IDbConnection conn, string queueTable);
	}
}
