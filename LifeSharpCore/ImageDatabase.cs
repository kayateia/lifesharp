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
public interface IImageDatabase
{
	/// <summary>
	/// Add new image to be processed. This will silently ignore if the item is already on the queue.
	/// </summary>
	void addToUploadQueue(string fullSourcePath, DateTimeOffset sendTimeout, string comment);

	/// <summary>
	/// Adds a newly downloaded image to the 
	/// </summary>
	/// <param name="image">Image.</param>
	void addDownloadedFile(string fullDownloadedPath, string filename, string userLogin, DateTimeOffset fileTime, string comment);

	/// <summary>
	/// Return a full path (whose parent dirs will exist on disk) for a scaled image. Note
	/// that for downloaded images, this will be in a "publicly" accessible place that gallery
	/// apps can reach, and for uploads, the files will be hidden.
	/// </summary>
	string getScaledPath(Image image);

	/// <summary>
	/// Returns an image by its Image.id value
	/// </summary>
	Image getImageById(int id);

	/// <summary>
	/// Returns an image by filename; note that if more than one matches, this will also
	/// return null.
	/// </summary>
	Image getImageByUserAndFileName(string userLogin, string filename);

	/// <summary>
	/// Returns a set of image specs that need processing. They will not be
	/// removed from the queue until markProcessed() has been called on each.
	/// </summary>
	Image[] getItemsToScale();

	/// <summary>
	/// Deletes an image by ID. Any associated image on disk will also be deleted.
	/// </summary>
	void deleteImage(int id);

	/// <summary>
	/// Pass in an Image.id that was returned from getItemToProcess() above, and this
	/// marks it as ready to send. You must do this after each item you've processed.
	/// </summary>
	void markReadyToSend(int id);

	/// <summary>
	/// Pass in an Image.id, and this marks it as sent and completed.
	/// </summary>
	void markSent(int id);

	/// <summary>
	/// Update the comment in the specified image, and put it back on the queue.
	/// </summary>
	void updateComment(int id, string comment);
}

// Represents one image on the queue.
public class Image
{
	public enum State
	{
		/// <summary>
		/// The image was just placed in the queue from the media scanner
		/// </summary>
		NewForUpload = 0,

		/// <summary>
		/// The image is scaled and ready to send; waiting on send timeout
		/// </summary>
		ReadyToSend = 1,

		/// <summary>
		/// The image was sent to the server and no longer needs any attention.
		/// </summary>
		Sent = 2,

		/// <summary>
		/// The image has already been sent, but the comments were updated and need re-sending
		/// </summary>
		CommentsUpdated = 3,

		/// <summary>
		/// The image was downloaded from LifeStream and needs no processing
		/// </summary>
		Downloaded = 4
	}

	/// <summary>
	/// Database ID of the image
	/// </summary>
	public int id { get; set; }

	/// <summary>
	/// State of the image (and type)
	/// </summary>
	public State state { get; set; }

	/// <summary>
	/// The filename stem of the image; this is typically the end component of sourcePath
	/// for uploaded images, but for downloads it will be the only hint of its filename.
	/// </summary>
	public string filename { get; set; }

	/// <summary>
	/// The full path to the source image, for uploads.
	/// </summary>
	public string sourcePath { get; set; }

	/// <summary>
	/// For downloaded images, the login of the user who posted it.
	/// </summary>
	public string userLogin { get; set; }

	/// <summary>
	/// Timestamp for when the item was placed in the queue (or downloaded)
	/// </summary>
	public DateTimeOffset queueStamp { get; set; }

	/// <summary>
	/// The time at which we will go ahead and send this image whether it has comments or
	/// not. This is ignored for downloads.
	/// </summary>
	public DateTimeOffset sendTimeout { get; set; }

	/// <summary>
	/// Comments associated with the image.
	/// </summary>
	public string comment { get; set; }
}


}

