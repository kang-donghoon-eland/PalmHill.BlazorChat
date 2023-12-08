﻿using Microsoft.KernelMemory;
using PalmHill.BlazorChat.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalmHill.LlmMemory
{
    public class ConversationMemory
    {
        public ConversationMemory(IKernelMemory kernelMemory)
        {
            KernelMemory = kernelMemory;
        }

        public IKernelMemory KernelMemory { get; }

        public ConcurrentDictionary<string, AttachmentInfo> AttachmentInfos { get; } = new ConcurrentDictionary<string, AttachmentInfo>();

        public async Task<AttachmentInfo> ImportDocumentAsync(
            AttachmentInfo attachmentInfo,
            Stream stream,
            TagCollection? tagCollection = null
            )
        {
            var documentId = await KernelMemory.ImportDocumentAsync(stream,
                attachmentInfo.Name,
                attachmentInfo.Id,
                tagCollection,
                attachmentInfo.ConversationId);

            if (documentId == null)
            {
                throw new Exception("Failed to import document");
            }

            attachmentInfo.Size = stream.Length;

            if (!AttachmentInfos.TryAdd(attachmentInfo.Id, attachmentInfo))
            { 
                throw new Exception("Failed to add attachment to memory");
            };


            return attachmentInfo;
        }

        public async Task<bool> IsAttachmetReady(string conversationId, string attachmentId)
        {
            var isDocReady = await KernelMemory.IsDocumentReadyAsync(conversationId, attachmentId);
            var attachmentInfo =  AttachmentInfos[attachmentId];
            attachmentInfo.Status = isDocReady ? AttachmentStatus.Uploaded : AttachmentStatus.Pending;
            
            return isDocReady;
        }

        public async Task<bool> DeleteDocument(string conversationId, string attachmentId)
        {
            await KernelMemory.DeleteDocumentAsync(attachmentId, conversationId);
            AttachmentInfos.Remove(attachmentId, out _);
            return true;
        }   

        public async Task<SearchResult> SearchAsync(string conversationId, string query)
        {
            var results = await KernelMemory.SearchAsync(query, conversationId);

            return results;
        }

        public async Task<MemoryAnswer> Ask(string conversationId, string query)
        {
            var results = await KernelMemory.AskAsync(query, conversationId);

            return results;
        }   

        public 


    }
}
