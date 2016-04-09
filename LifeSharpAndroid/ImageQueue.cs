/*
    LifeStream - Instant Photo Sharing
    Copyright (C) 2014-2016 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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

