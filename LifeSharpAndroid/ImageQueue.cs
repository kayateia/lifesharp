/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;

namespace LifeSharp
{

// Database interface for managing images on the processing queue.
public interface IImageQueue
{
	// Add new image to be processed. This will silently ignore if the item is already on the queue.
	void addToQueue(string imageName);

	// Returns a set of image specs that need processing. They will not be
	// removed from the queue until markProcessed() has been called on each.
	Image[] getItemsToProcess();

	// Pass in an int that was returned from getItemToProcess() above, and this
	// marks it as skipped. It will be sent to the end of the queue. You must do
	// this after each item you don't want to or can't process right now.
	void markSkipped(int id);

	// Pass in an int that was returned from getItemToProcess() above, and this
	// marks it as completed, no longer on the queue. You must do this after each
	// item you've processed.
	void markProcessed(int id);
}

// Represents one image on the queue.
public class Image
{
	public int id { get; set; }
	public string pathname { get; set; }
	public int timestamp { get; set; }
	public int queuestamp { get; set; }
}


}

