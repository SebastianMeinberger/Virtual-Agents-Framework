using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.OpenAI;
using System.Threading.Tasks;
using System.Linq;
using Codice.Client.Common.Connection.ServerAlias;

namespace i5.VirtualAgents.AgentTasks
{
    public class LLMImageCoordinates : ILLMFunction
    {
        public int x;
        public int y;

        public LLMImageCoordinates()
        {
            functionName = "PointAtCoordinate";
        }

        public override string Work()
        {
            Debug.Log("Coordinates: " + x + " " + y);
            return "Sucess";
        }

    }

    public class LLMPoint : Point
    {
        protected GameObject canvas;
        // Arguments for function call
        public string imageDescription;
        public string imageName;

        public int x_pos;
        public int y_pos;

        private Agent agent;
        public override void StartExecution(Agent agent)
        {
            this.agent = agent;
            // Debug
            //imageDescription = "Green button \'+ Project\' on the system\'s home screen to start creating a new project.";
            //imageName = "create-a-new-project-landing-page.png";


            var request = new Request();
            var input = new TextInput();
            var text = new TextContent();
            ImageContent image = new ImageContent();
            image.file_id = imageIDs[imageName];
            input.content = new Content[] { text, image };
            request.input = request.input.Append(input).ToArray();

            var llmPoint = new FunctionCall();
            llmPoint.name = "PointAtCoordinate";
            llmPoint.description = "Point at a pixel coordinate";
            llmPoint.parameters.properties = new List<PropertieTemplate>
            {
                new PropertieTemplate("x", "number", "The x coordinate of the described position"),
                new PropertieTemplate("y", "number", "The y coordinate of the described position")
            };
            //llmPoint.parameters.required = new string[] { "x", "y" };

            request.tools = new Tool[] { llmPoint };
            text.text = "Using a tool call, point out the pixel coodinates that fit the following description: " + imageDescription + " Pixel Coordinates start at x:0 y:0 and end at x:image width and y:image height.";
            Debug.Log(request.ToJson());
            agent.StartCoroutine(WaitForRequest(request));
        }

        IEnumerator WaitForRequest(Request request)
        {
            var coordinates = new LLMImageCoordinates();
            yield return RequestHandler.Upload(request, new ILLMFunction[] { coordinates }, false);
            position = ImagePosToSpritePos(coordinates.x, coordinates.y);
            Debug.Log(position);
            base.StartExecution(agent);
        }

        protected Vector3 ImagePosToSpritePos(int xImage, int yImage)
        {
            var spriteRenderer = canvas.GetComponent<SpriteRenderer>();
            int imageHeight = spriteRenderer.sprite.texture.height;
            int imageWidth = spriteRenderer.sprite.texture.width;
            float spriteHeight = spriteRenderer.localBounds.extents.y;
            float spriteWith = spriteRenderer.localBounds.extents.x;
            float linearTransform(int imageCoord, int imageSize, float spriteSize, int mult)
            {
                return (imageCoord - imageSize / 2) * mult * 2 * spriteSize / imageSize;
            }
            float xSprite = linearTransform(xImage, imageWidth, spriteWith, 1);
            float ySprite = linearTransform(yImage, imageHeight, spriteHeight, -1);
            return canvas.transform.localToWorldMatrix.MultiplyPoint(new Vector3(xSprite, ySprite, 0));
        }

        public override void Serialize(SerializationDataContainer serializer)
        {
            serializer.AddSerializedData("Canvas", canvas);
            serializer.AddSerializedData("Image Description", imageDescription, "A description of the point on the image that should be pointed at");
            serializer.AddSerializedData("Image Name", imageName, "The name of an image that is referenced in one of the provided markdown files.");
        }

        public override void Deserialize(SerializationDataContainer serializer)
        {
            canvas = serializer.GetSerializedGameobjects("Canvas");
            imageDescription = serializer.GetSerializedString("Image Description");
            imageName = serializer.GetSerializedString("Image Name");
        }

        protected Dictionary<string, string> imageIDs = new()
        {
            {"name-your-project.png", "file-Hut6qHN1c1fjtnyxUed9XW"},
            {"view_all_projects_options.png", "file-MqADtR9iXrJZsyUgGBpkPs"},
            {"openproject-landing-page.png", "file-GXy1QPMiHGTZ1wL6iTe2nz"},
            {"create-a-new-project-landing-page.png", "file-5A81j2PDzZB1KNnVFpkZp1"},
            {"view_all_projects.png", "file-FGLYH92SicBMttMgQ6NJm5"},
            {"project-overview-list.png", "file-Cxhydf6gaYQuf9Z7kTedXu"},
            {"project_hierarchy-8178054.png", "file-71csfCeQ8JnznMi5NRqcCP"},
            {"filter_project_header_menu.png" ,"file-UW6D3ETEgTV1j4H14tffAf"},
            {"create-project-header.png", "file-5jgAXJnmtyQCqgGQtpn7nX"},
            {"split-screen-workpackages.png", "file-8qm2q4kVcv5V1CNvAzHZtY"},
            {"open-details-view-work-packages.png", "file-T6KHq7e13LgeBkb6wuPyRm"},
            {"create-work-package.png", "file-FWQrK4vEumLzxkL5fN68jS"},
            {"create-work-package-header.png", "file-9GSQvpRH1fGppV71csYFiV"},
            {"create-work-package-define-project-6669224.png", "file-GgLWyxHuvs7U5EKyrPvM33"},
            {"create-a-new-workpackage.png", "file-C8iFqjVee2GmYSzkNKVx7f"},
            {"activity-work-packages.png", "file-7cC2gWMDsdwf1hi1YWQKJL"},
            {"1569612428626.png", "file-MH3tw9aMCsyxmpiYJoLiDZ"},
            {"1569612205009.png", "file-BMZCTCeiLUcmaPv5q6bwx5"}
        };

        // TODO Replace with CSV reader
        protected string imageNameToID() => imageName switch
        {
            "name-your-project.png" => "file-Hut6qHN1c1fjtnyxUed9XW",
            "view_all_projects_options.png" => "file-MqADtR9iXrJZsyUgGBpkPs",
            "openproject-landing-page.png" => "file-GXy1QPMiHGTZ1wL6iTe2nz",
            "create-a-new-project-landing-page.png" => "file-5A81j2PDzZB1KNnVFpkZp1",
            _ => throw new System.Exception("OpenAI stored image not found")
        };
    }
}
